# DogeMood

The website is very simple, but I had to use many features to make it work as expected. 
Front end might be buggy and look bad, I did not really bother to make it look fancy.

So you can browse the code to find examples of the following:

1. TimedHostedService
2. Reddit API usage, getting token
3. ScopedService and how to get other stuff using it
4. Table splitting and many-to-many example (ef core)
5. Basic work with images
6. SweetAlert js library usage inside .net core page
7. One useful taghelper
8. Calling controller method from js code with parameters and using return data
9. Unit testing with mocking of db context, hosting service, scoped service,
     current user and printing some info to output. Also some integration tests.
10. Log to file using serilog.
11. Usage of user secrets in code and during tests.
12. Small example of IoC/DI
13. Unit test of file upload (by file in context and by URL in parameters)
14. Custom exception handler for production

Also I was learning git workflow and one merge went wrong, the top branch is now not "master". 

How it works in general:
- downloads 10 top pics from subreddit each day and stores links in database. Pictogram of image is stored as byte[].
- users can browse for pictures and "like" them, 4 pics at page. User dont have to login/register for "like".
- if user want to favorite the image, he/she must register and login. Favorite image is stored in db as byte[].
- user can upload image via file/link; but image needs to be moderated by an admin.
- moderator/admin can look for images, delete and approve.