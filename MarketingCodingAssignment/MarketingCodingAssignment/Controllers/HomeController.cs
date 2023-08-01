using MarketingCodingAssignment.Models;
using MarketingCodingAssignment.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using Lucene.Net.Search;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarketingCodingAssignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SearchEngine _searchEngine;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _searchEngine = new SearchEngine();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        [HttpGet]
        public JsonResult AutoCompleteSearch(string searchString)
        {
            HashSet<string> movies = new HashSet<string>();
            try
            {
                movies = _searchEngine.AutoCompleteSearch(searchString);
            }
            catch(Exception ex)
            {
                movies = null;
            }
            
            //var movieTitles = movies.Where(m => m.Contains(searchString)).ToList();
            return Json(movies);
        }
        [HttpGet]
        public JsonResult Search(String searchString)
        {
            var searchResults = new List<Film>();
            var movieTitles = new List<Film>();
            try
            {
                searchResults = _searchEngine.SearchMovies(searchString);
                movieTitles = searchResults.Where(m => m.Title.ToLower().Contains(searchString.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                movieTitles = null;
            } 
            
            return Json(new { responseCode = 0, films = movieTitles });
        }

        public JsonResult SearchFilter(string searchString, string filterColumn, string filterValue,string LoadAct, int pageNumber, int pageSize)
        {
            var films = new List<Film>();
            try
            {
                if (LoadAct == "initial")
                {
                    var movies = _searchEngine.ReadMoviesFromCsv();
                    films = _searchEngine.GetMoviesForPage(movies, pageNumber, pageSize);
                }
                if (LoadAct == "filter")
                {

                    if (filterColumn == "searchValue")
                    {
                        var searchResults = _searchEngine.SearchMovies(searchString);
                        films = searchResults.Where(m => m.Title.ToLower().Equals(searchString.ToLower())).ToList();
                    }
                    else
                    {
                        object parsedFilterValue = filterValue;
                        if (!string.IsNullOrEmpty(filterColumn))
                        {
                            switch (filterColumn)
                            {
                                case "ReleaseDate":
                                    if (DateTime.TryParse(filterValue, out var parsedDate))
                                    {
                                        parsedFilterValue = parsedDate;
                                    }
                                    else
                                    {
                                        parsedFilterValue = null;
                                    }
                                    break;
                                case "Rating":
                                    if (double.TryParse(filterValue, out var parsedRating))
                                    {
                                        parsedFilterValue = parsedRating;
                                    }
                                    else
                                    {
                                        parsedFilterValue = null;
                                    }
                                    break;
                            }
                        }

                        films = _searchEngine.SearchFilter(searchString, filterColumn, parsedFilterValue, pageNumber, pageSize);
                    }

                }
            }
            catch (Exception ex)
            {
                films = null;
            }
            
            return Json(new { responseCode = 0, films });

        }

        //public void DeleteIndexContents()
        //{
        //    _searchEngine.DeleteIndexContents();
        //    return;
        //}

    }
}