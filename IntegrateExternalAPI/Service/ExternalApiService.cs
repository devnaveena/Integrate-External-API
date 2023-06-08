using IntegrateExternalAPI.Repository;
using IntegrateExternalAPI.Models;
using IntegrateExternalAPI.Data;
using Microsoft.Extensions.Logging;
using DocumentFormat.OpenXml;
using IntegrateExternalAPI.Contracts;
using DocumentFormat.OpenXml.Packaging;

namespace IntegrateExternalAPI.Service
{

    public class ExternalApiService : IExternalApiService
    {
        private readonly IExternalApiRepository _externalAPIRepository;
        private readonly ILogger<ExternalApiService> _logger;
        private readonly IExportToExcel _exportToExcel;

        public ExternalApiService(IExternalApiRepository externalAPIRepository, ILogger<ExternalApiService> logger, IExportToExcel exportToExcel)
        {
            _externalAPIRepository = externalAPIRepository;
            _logger = logger;
            _exportToExcel = exportToExcel;
        }

        /// <summary>
        /// This function retrieves user details from an external API, generates an Excel file with user,
        /// post, comment, and todos data, and saves it to a specified file path.
        /// </summary>

        public async Task<string> ExternalService()
        {

            _logger.LogInformation("Getting all user details");
            List<User> userList = await _externalAPIRepository.GetAsync<User>("users");//Getting User list

            string filePath = string.Empty;

            //Iterating each user data in parallel
            List<Task> tasks = new List<Task>();
            foreach (var user in userList)
            {
                var task = Task.Run(async () =>
                {
                    _logger.LogInformation($"Retrieving details for user with ID {user.Id}");
                    User userResult = await _externalAPIRepository.GetByIdAsync<User>(user.Id, "users");
                    List<Post> postList = await _externalAPIRepository.GetByIdAsync<Post>(user.Id, "users", "posts");

                    var commentList = new List<Comment>();
                    List<List<Comment>> commentsLists = new();

                    if (postList.Count == 0)
                    {
                        _logger.LogInformation("Generating Random post for a user");
                        postList = await GenerateRandomPost(user);

                        foreach (var post in postList)
                        {
                            commentList = await GenerateRandomComments(post, userList);
                            commentsLists.Add(commentList);
                        }
                    }
                    else
                    {
                        foreach (var post in postList)
                        {
                            commentList = _externalAPIRepository.GetByIdAsync<Comment>(post.Id, "posts", "comments").Result;
                            commentsLists.Add(commentList);
                        }

                        if (commentList.Count == 0)
                        {
                            _logger.LogInformation("Generating Random Comments for a user");
                            foreach (var post in postList)
                            {
                                commentList = await GenerateRandomComments(post, userList);
                                commentsLists.Add(commentList);
                            }
                        }
                    }
                    var todoList = await _externalAPIRepository.GetByIdAsync<Todo>(user.Id, "users", "todos");
                    //Writing data to Excel 
                    filePath = _exportToExcel.ExportDataToExcel(userResult, postList, commentsLists, todoList.OrderBy(a => a.DueOn).ToList());
                });

                tasks.Add(task);
            }
            await Task.WhenAll(tasks); // Wait for all tasks to complete before returning the file path
            return filePath;

        }



        /// <summary>
        /// This function generates a list of 5 random posts for a given user.
        /// </summary>
        /// <param name="userList">The User parameter is a list of users from which a random user will be
        /// selected to create a post.</param>
        /// <returns>
        /// The method returns a list of 5 randomly generated Post objects.
        /// </returns>

        private async Task<List<Post>> GenerateRandomPost(User userList)
        {
            List<Post> posts = new List<Post>();
            int postId = 0;
            while (posts.Count < 5)
            {
                Post post = new Post() { Id = 1, Body = $"This is {++postId} the post.", Title = $" Post of the day {postId}", UserId = userList.Id };
                var postsList = await _externalAPIRepository.PostAsync<Post>(post, userList.Id, "users", "posts");
                posts.Add(postsList);
            }
            return posts;
        }


        /// <summary>
        /// This function generates a list of 10 random comments for a given post.
        /// </summary>
        /// <param name="postList">The postList parameter is a list of Post objects that the comments will be
        /// generated for.</param>
        /// <returns>
        /// The method is returning a list of 10 randomly generated comments for a given post.
        /// </returns>
        private async Task<List<Comment>> GenerateRandomComments(Post postList, List<User> userList)
        {
            var random = new Random();
            List<Comment> commentList = new List<Comment>();
            int commentId = 0;
            while (commentList.Count < 10)
            {
                int randomUserId = userList[random.Next(userList.Count)].Id;
                List<Post> postLists = await _externalAPIRepository.GetByIdAsync<Post>(randomUserId, "users", "posts");
                foreach (var post in postLists)
                {
                    List<Comment> commentLists = await _externalAPIRepository.GetByIdAsync<Comment>(post.Id, "posts", "comments");
                    foreach (var comment in commentLists)
                    {
                        Comment comments = new Comment { Id = commentId++, Email = comment.Email, Body = comment.Body, Name = comment.Name, PostId = postList.Id };
                        Comment commentResult = await _externalAPIRepository.PostAsync<Comment>(comments, postList.Id, "posts", "comments");
                        commentList.Add(commentResult);
                    }
                }
            }
            return commentList;
        }
    }
}
