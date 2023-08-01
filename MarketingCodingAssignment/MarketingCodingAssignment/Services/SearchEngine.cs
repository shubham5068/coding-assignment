using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using MarketingCodingAssignment.Models;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Lucene.Net.QueryParsers.Classic;

namespace MarketingCodingAssignment.Services
{
    public class SearchEngine
    {
        // The code below is roughly based on sample code from: https://lucenenet.apache.org/

        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly string _indexDirectory = @"C:\LuceneIndex";
        private readonly Lucene.Net.Store.Directory _directory;
        private readonly Analyzer _analyzer;
        private readonly IndexWriterConfig _indexConfig;
        private readonly IndexWriter _indexWriter;

        public SearchEngine()
        {
            if (!System.IO.Directory.Exists(_indexDirectory))
                System.IO.Directory.CreateDirectory(_indexDirectory);
            _directory = FSDirectory.Open(new DirectoryInfo(_indexDirectory));
            _analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
            _indexConfig = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, _analyzer);
            _indexConfig.WriteLockTimeout = 60000; // Set the write lock timeout in milliseconds (e.g., 10000 ms = 10 seconds).
            _indexWriter = new IndexWriter(_directory, _indexConfig);

            var movies = ReadMoviesFromCsv();
            CreateIndex(movies);

        }

        public List<Film> ReadMoviesFromCsv()
        {
            var records = new List<Film>();
            string csvFilePath = $"{System.IO.Directory.GetCurrentDirectory()}{@"\wwwroot\files"}" + "\\" + "MoviesList.csv";
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                records = csv.GetRecords<Film>().OrderByDescending(x=>x.VoteAverage).ThenByDescending(x => x.ReleaseDate).ToList();
                
            }
            using (StreamReader r = new StreamReader(csvFilePath))
            {
                var allFileText = r.ReadToEnd();
            }
            return records;

        }

        public void CreateIndex(List<Film> movies)
        {
            foreach (var movie in movies)
            {
                var doc = new Document
            {
                new TextField("Id", movie.Id, Field.Store.YES),
                new TextField("Title", movie.Title, Field.Store.YES),
                 new TextField("Overview", movie.Overview, Field.Store.YES),
                 new TextField("ReleaseDate", movie.ReleaseDate, Field.Store.YES),
                 new TextField("VoteAverage", movie.VoteAverage, Field.Store.YES),
                 new TextField("Genres", movie.VoteAverage, Field.Store.YES)
            };
                _indexWriter.AddDocument(doc);
            }

            _indexWriter.Flush(true, true);
            _indexWriter.Commit();
            _indexWriter.Dispose();
        }
        public List<Film> GetMoviesForPage(List<Film> MoviesList, int pageNumber, int pageSize)
        {
            int skipRecords = (pageNumber - 1) * pageSize;

            List<Film> movies = MoviesList
                .OrderByDescending(f => f.VoteAverage).ThenByDescending(f => f.ReleaseDate)
                .Skip(skipRecords)
                .Take(pageSize)
                .ToList();

            return movies;
        }

        public List<Film> SearchMovies(string query)
        {
            var result = new List<Film>();
            var uniqueMovieIds = new HashSet<string>();
            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);
                var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Title", _analyzer);
                var luceneQuery = parser.Parse(query);

                var topDocs = searcher.Search(luceneQuery, 100);

                foreach (var scoreDoc in topDocs.ScoreDocs)
                {
                    var doc = searcher.Doc(scoreDoc.Doc);
                    var movieId = doc.Get("Id");
                    if (uniqueMovieIds.Contains(movieId))
                    {
                        continue;
                    }
                    uniqueMovieIds.Add(movieId);
                    var movie = new Film
                    {
                        Id = doc.Get("Id"),
                        Title = doc.Get("Title"),
                        Overview = doc.Get("Overview"),
                        ReleaseDate = doc.Get("ReleaseDate"),
                        VoteAverage = doc.Get("VoteAverage"),
                        Genres = doc.Get("Genres")
                    };
                    result.Add(movie);
                }
            }
            _indexWriter.Dispose();
            return result;
        }


        public HashSet<string> AutoCompleteSearch(string query)
        {
            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);
                var autoCompleteField = "Title"; 

                var autoCompleteQuery = new PrefixQuery(new Term(autoCompleteField, query.ToLowerInvariant()));
                var topDocs = searcher.Search(autoCompleteQuery, 10);

                var suggestions = new HashSet<string>();
                foreach (var scoreDoc in topDocs.ScoreDocs)
                {
                    var doc = searcher.Doc(scoreDoc.Doc);
                    var suggestion = doc.Get(autoCompleteField);
                    suggestions.Add(suggestion);
                }
                _indexWriter.Dispose();
                return suggestions;
            }
            
        }

        public List<Film> SearchFilter(string query, string filterColumn, object filterValue, int pageNumber, int pageSize)
        {
            var result = new List<Film>();
            var uniqueMovieIds = new HashSet<string>();
            var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, filterColumn, _analyzer);

            if (!string.IsNullOrEmpty(query))
            {
                var luceneQuery = parser.Parse(query);
                var filterQuery = CreateFilterQuery(filterColumn, filterValue);

                if (filterQuery != null)
                {
                    var booleanQuery = new BooleanQuery();
                    booleanQuery.Add(luceneQuery, Occur.MUST);
                    booleanQuery.Add(new BooleanClause(filterQuery, Occur.MUST_NOT));

                    PerformSearch(booleanQuery, uniqueMovieIds, result, pageNumber, pageSize);
                }
                else
                {
                    PerformSearch(luceneQuery, uniqueMovieIds, result, pageNumber, pageSize);
                }
            }
            else
            {
                var filterQuery = CreateFilterQuery(filterColumn, filterValue);
                if (filterQuery != null)
                {
                    PerformSearch(filterQuery, uniqueMovieIds, result, pageNumber, pageSize);
                }
                else
                {
                    PerformSearch(null, uniqueMovieIds, result, pageNumber, pageSize);
                }
            }

            return result;
        }

        private Query CreateFilterQuery(string filterColumn, object filterValue)
        {
            if (string.IsNullOrEmpty(filterColumn) || filterValue == null)
            {
                return null;
            }

            switch (filterColumn)
            {
                case "ReleaseDate":
                    if (DateTime.TryParse(filterValue.ToString(), out var releaseDate))
                    {
                        var formattedDate = releaseDate.ToString("yyyy-MM-dd");
                        return new TermQuery(new Term("ReleaseDate", formattedDate));
                    }
                    break;
                case "Rating":
                    if (double.TryParse(filterValue.ToString(), out var rating))
                    {
                        return NumericRangeQuery.NewDoubleRange("VoteAverage", rating, rating, true, true);
                    }
                    break;
                case "Genres":
                    return new TermQuery(new Term("Genres", filterValue.ToString()));
                    break;
            }

            return null;
        }

        private void PerformSearch(Query query, HashSet<string> uniqueMovieIds, List<Film> result, int pageNumber, int pageSize)
        {
            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);
                var topDocs = searcher.Search(query, (pageNumber - 1) * pageSize + pageSize);

                for (int i = (pageNumber - 1) * pageSize; i < topDocs.ScoreDocs.Length; i++)
                {
                    var scoreDoc = topDocs.ScoreDocs[i];
                    var doc = searcher.Doc(scoreDoc.Doc);
                    var movieId = doc.Get("Id");
                    if (uniqueMovieIds.Contains(movieId))
                    {
                        continue; 
                    }

                    uniqueMovieIds.Add(movieId);
                    var movie = new Film
                    {
                        Id = doc.Get("Id"),
                        Title = doc.Get("Title"),
                        Overview = doc.Get("Overview"),
                        ReleaseDate = doc.Get("ReleaseDate"),
                        VoteAverage = doc.Get("VoteAverage"),
                        Genres = doc.Get("Genres")
                    };
                    result.Add(movie);

                    if (result.Count == pageSize)
                    {
                        break;
                    }
                }
            }
        }

        //public void DeleteIndexContents()
        //{
        //    var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        //    var indexPath = Path.Combine(basePath, "index");
        //    using var dir = FSDirectory.Open(indexPath);
        //    var analyzer = new StandardAnalyzer(AppLuceneVersion);
        //    var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
        //    using var writer = new IndexWriter(dir, indexConfig);
        //    writer.DeleteAll();
        //    writer.Commit();
        //    writer.Dispose();
        //    return;
        //}

    }
}
