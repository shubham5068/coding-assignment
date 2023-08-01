# coding-assignment

I have built a web-based search application, MovieHub, using Lucene.NET and .NET Core. This application allows users to search for movies using keywords and is backed by test data from the included movies.csv file. In addition to basic search functionality, I have implemented several advanced features to enhance the user experience.

## Basic Features Implemented

1. **Data Indexing**: The application loads the test data from movies.csv and creates an index using Lucene.NET. This indexing process ensures faster and more efficient searching of movie data.

2. **Keyword Search**: Users can enter movie titles to perform a search. The application retrieves relevant movie information based on the user's input.

3. **Responsive Design**: MovieHub features a responsive design, ensuring that it renders well on various devices, including desktops, tablets, and mobile phones. Users can access the search app seamlessly from different screen sizes and resolutions.

## Advanced Features Implemented

1. **Faceting**: Users can filter search results based on the film rating.

2. **Stemming**: The search functionality employs stemming, enabling the application to return results for related terms when a user enters a search query. For example, when searching for "engineer," the app also returns results for "engineering," "engineers," and "engineered."

3. **Spell Checking**: MovieHub presents corrected search terms for user misspellings. If a user enters a misspelled keyword, the app suggests the correct term, improving the accuracy of search results.

4. **Auto-Complete**: As users type in the search box, MovieHub provides auto-complete suggestions for search terms. This feature assists users in finding the desired movie titles quickly and efficiently.

## Process for Solving the Problem

1. **Research and Learning**: I familiarized myself with Lucene.NET and its integration with .NET Core. I studied the basic and advanced features required for the web-based search application.

2. **Project Setup**: I created a new .NET Core web application project and added the required dependencies for Lucene.NET.

3. **Data Indexing**: I implemented the code to read data from movies.csv and index it using Lucene.NET.

4. **Basic Search**: I added functionality for users to enter keywords and perform a basic search on the indexed data.

5. **Responsive Design**: I used Bootstrap and CSS to ensure a responsive design that adapts to various screen sizes and resolutions.

6. **Advanced Features**: I implemented the advanced features, including faceting, stemming, spell checking, related search terms, and auto-complete.

7. **Testing and Refinement**: I thoroughly tested the application to ensure all features work as expected. I made necessary refinements and bug fixes during this process.

8. **Documentation**: I updated this README file to describe the features implemented and my approach to solving the problem.

## Feedback and Improvements

I approached the problem by conducting thorough research on Lucene.NET and its integration with .NET Core. During the development process, I managed my time effectively to prioritize implementing the basic features first and then tackled the more complex advanced features. I also conducted extensive testing to ensure that all features work as expected and refined the application based on the feedback received. Sometimes the application performs a little slow because of a large amount of data, such as nearly 45,000 records in the Excel file.  

The end result is an easy-to-use and visually pleasing web-based search application, MovieHub, that empowers users to find movies based on their preferences. I enjoyed exploring new technologies and designing a user-friendly application that showcases my problem-solving skills and ability to learn and adapt to new challenges.

# Deploying and Running the Application

To deploy and run the application, follow these steps:

1. **Build the Application**:
   - Make sure you have .NET Core SDK installed on your deployment server or hosting environment.
   - Copy the entire application folder or publish the application to a target directory.

2. **Set Up the Hosting Environment**:
   - Ensure that your hosting environment supports .NET Core applications. This may be a web server like IIS or a cloud hosting platform like Azure or AWS.

3. **Install Dependencies**:
   - If your application uses any external dependencies or packages, make sure they are installed on the hosting environment.

4. **Configure Lucene.NET Index Directory**:
   - If your application stores the Lucene.NET index locally, ensure that the index directory has appropriate read and write permissions for the web application.

5. **Deploy/Publish the Application**:
   - Copy the published application files or upload the application folder to the hosting environment.

6. **Start the Application**:
   - If using IIS, create a new website or application pool and configure it to run the .NET Core application.
   - If using a cloud hosting platform, follow the platform-specific instructions to deploy and start the application.

7. **Access the Application**:
   - Once the application is deployed/published and running, access it through the assigned URL or domain name.

If you have any feedback or questions about the implementation, I am more than happy to discuss and further improve the application.

