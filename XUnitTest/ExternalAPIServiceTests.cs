using IntegrateExternalAPI.Repository;
using IntegrateExternalAPI.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using IntegrateExternalAPI.Models;
using IntegrateExternalAPI.Data;
using Moq;
using Microsoft.Extensions.Logging;
using IntegrateExternalAPI.Contracts;
namespace IntegrateExternalAPI.Tests
{
    public class ExternalAPIServiceTests
    {
        private ExternalApiRepository _repository;
        private ExternalApiService _service;
        private ExportToExcel _exportToExcel;
        private Mock<IApiClient> _mockApiClient;
        private Mock<ILogger<ExternalApiRepository>> _repositoryLogger;
        private Mock<ILogger<ExternalApiService>> _serviceLogger;
        private Mock<ILogger<ExportToExcel>> _log;

        /* The above code is creating an instance of the ExternalAPIServiceTests class and initializing
        its fields. It is creating mock objects for the IApiClient, ILogger, and ExportToExcel
        interfaces, and then creating instances of the ExternalAPIRepository and ExternalAPIService
        classes using these mock objects. */
        public ExternalAPIServiceTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _repositoryLogger = new Mock<ILogger<ExternalApiRepository>>();
            _serviceLogger = new Mock<ILogger<ExternalApiService>>();
            _log = new Mock<ILogger<ExportToExcel>>();
            _exportToExcel = new ExportToExcel(_log.Object);
            _repository = new ExternalApiRepository(_mockApiClient.Object, _repositoryLogger.Object);
            _service = new ExternalApiService(_repository, _serviceLogger.Object, _exportToExcel);
        }

        /// <summary>
        /// This is a unit test for a C# function that tests if an external service can create an Excel
        /// file for a user with posts, comments, and todos.
        /// </summary>
        [Fact]
        public async Task ExternalService_WhenUserWithPostAndCommentsAvailable_CreateExcelFile()
        {
            List<User> users = new List<User>
           {
            new User {Id = 1,Name = "John Doe",Email = "johndoe@example.com",Gender = "Male",Status = "Active"  },
           };

            User user = new User
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Gender = "male",
                Status = "active"
            };

            //Creating Post for  Users
            List<Post> posts = new List<Post>()
          {
            new Post(){  Id = 1,  Body = "This is the first post.",  Title = "First Post",  UserId = 1},
          };

            //Creating Comment for users
            List<Comment> comments = new List<Comment>
          {
            new Comment{ Id = 1, Email = "john@example.com", Body = "This is a great post!", Name = "John Doe", PostId = 1  },
          };

            //Creating Todo for user
            List<Todo> todos = new List<Todo>
         {
            new Todo { Id = 1, Title = "Complete project", UserId = 1, DueOn = new DateTime(2023, 04, 30) },
            new Todo { Id = 2, Title = "Finish presentation", UserId = 1, DueOn = new DateTime(2023, 05, 10) },
         };

            _mockApiClient.Setup(a => a.GetList<User>("/public/v2/users")).ReturnsAsync(users);
            _mockApiClient.Setup(a => a.Get<User>("/public/v2/users/1")).ReturnsAsync(user);
            _mockApiClient.Setup(a => a.GetList<Post>("/public/v2/users/1/posts")).ReturnsAsync(posts);
            _mockApiClient.Setup(a => a.GetList<Comment>("/public/v2/posts/1/comments")).ReturnsAsync(comments);
            _mockApiClient.Setup(a => a.GetList<Todo>("/public/v2/users/1/todos")).ReturnsAsync(todos);

            //Act
            var result = await _service.ExternalService();

            //Assert
            Assert.True(Directory.Exists(Directory.GetCurrentDirectory() + "\\File\\"));
            Assert.True(File.Exists(result));
            File.Delete(result);
        }

        /// <summary>
        /// This is a unit test for a function that tests if an external service can generate and create
        /// an Excel file when a user with posts and comments is not available.
        /// </summary>
        [Fact]
        public async Task ExternalService_WhenUserWithPostAndCommentsNotAvailable_GernerateAndCreateExcelFile()
        {
            List<User> users = new List<User>
         {
           new User {Id = 1,Name = "John Doe",Email = "johndoe@example.com",Gender = "Male",Status = "Active" },
         };
            User user = new User
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Gender = "male",
                Status = "active"
            };
            //Creating Post of Users
            List<Post> posts = new List<Post>();
            List<Post> postList = new List<Post>
            {
              new Post(){ Id = 1,Body = $"This is the post.",Title = $" Post",UserId = 1},
              new Post(){ Id = 1,Body = $"This is the post.",Title = $" Post",UserId = 1},
              new Post(){ Id = 1,Body = $"This is the post.",Title = $" Post",UserId = 1},
              new Post(){ Id = 1,Body = $"This is the post.",Title = $" Post",UserId = 1},
              new Post(){ Id = 1,Body = $"This is the post.",Title = $" Post",UserId = 1},
            };

            Post data = new Post()
            {
                Id = 1,
                Body = $"This is the post.",
                Title = $" Post",
                UserId = 1
            };
            //Creating Comment for users
            List<Comment> comments = new List<Comment>();
            List<Comment> postComments = new List<Comment>
             {
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
             };
            Comment comment = new Comment()
            {
                Id = 1,
                Email = "S@gmail.com",
                Body = "First Comment",
                Name = "S",
                PostId = 1
            };
            //Creating Todo for user
            List<Todo> todos = new List<Todo>
         {
           new Todo { Id = 1, Title = "Complete project", UserId = 1, DueOn = new DateTime(2023, 04, 30) },
           new Todo { Id = 2, Title = "Finish presentation", UserId = 1, DueOn = new DateTime(2023, 05, 10) },
         };
            _mockApiClient.Setup(a => a.GetList<User>("/public/v2/users")).ReturnsAsync(users);
            _mockApiClient.Setup(a => a.Get<User>("/public/v2/users/1")).ReturnsAsync(user);
            var setup = _mockApiClient.SetupSequence(a => a.GetList<Post>("/public/v2/users/1/posts"));
            setup.ReturnsAsync(posts);
            for (int i = 0; i < 10; i++)
            {
                setup.ReturnsAsync(postList);
            }
            setup.ReturnsAsync(posts);

            var setup1 = _mockApiClient.SetupSequence(a => a.GetList<Comment>("/public/v2/posts/1/comments"));
            for (int i = 0; i < 50; i++)
            {
                setup1.ReturnsAsync(postComments);
            }
            // setup.ReturnsAsync(posts);
            _mockApiClient.Setup(a => a.PostData<Post, Post>("/public/v2/users/1/posts", It.IsAny<Post>())).ReturnsAsync(data);
            _mockApiClient.Setup(a => a.PostData<Comment, Comment>("/public/v2/posts/1/comments", It.IsAny<Comment>())).ReturnsAsync(comment);
            _mockApiClient.Setup(a => a.GetList<Todo>("/public/v2/users/1/todos")).ReturnsAsync(todos);

            //Act
            var result = await _service.ExternalService();

            //Assert
            Assert.True(Directory.Exists(Path.Combine(Directory.GetCurrentDirectory() + "\\File\\")));
            Assert.True(File.Exists(result));
            File.Delete(result);

        }
        /// <summary>
        /// This is a unit test for a function that tests the creation of an Excel file for a user with a
        /// post and comments available, using mocked API data.
        /// </summary>
        [Fact]
        public async Task ExternalService_WhenUserWithPostAvailableCommentsNotAvailable_GernerateAndCreateExcelFile()
        {
            //Arrange

            List<User> users = new List<User>
         {
           new User {Id = 1,Name = "John Doe",Email = "johndoe@example.com",Gender = "Male",Status = "Active" },
         };
            User user = new User
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Gender = "male",
                Status = "active"
            };
            //Creating Post of Users
            List<Post> posts = new List<Post>()
          {
            new Post(){  Id = 1,  Body = "This is the first post.",  Title = "First Post",  UserId = 1},
          };
            Post data = new Post()
            {
                Id = 1,
                Body = $"This is the post.",
                Title = $" Post",
                UserId = 1
            };
            //Creating Comment for users
            List<Comment> comments = new List<Comment>();
            List<Comment> postComments = new List<Comment>
             {
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
              new Comment{  Id = 1,  Email = "S@gmail.com",  Body = "First Comment",  Name = "S",  PostId = 1},
             };

            Comment comment = new Comment()
            {
                Id = 1,
                Email = "S@gmail.com",
                Body = "First Comment",
                Name = "S",
                PostId = 1
            };
            //Creating Todo for user
            List<Todo> todos = new List<Todo>
         {
           new Todo { Id = 1, Title = "Complete project", UserId = 1, DueOn = new DateTime(2023, 04, 30) },
           new Todo { Id = 2, Title = "Finish presentation", UserId = 1, DueOn = new DateTime(2023, 05, 10) },
         };
            _mockApiClient.Setup(a => a.GetList<User>("/public/v2/users")).ReturnsAsync(users);
            _mockApiClient.Setup(a => a.Get<User>("/public/v2/users/1")).ReturnsAsync(user);
            _mockApiClient.Setup(a => a.GetList<Post>("/public/v2/users/1/posts")).ReturnsAsync(posts);
            _mockApiClient.SetupSequence(a => a.GetList<Comment>("/public/v2/posts/1/comments"))
                  .ReturnsAsync(comments)
                  .ReturnsAsync(postComments);
            _mockApiClient.Setup(a => a.PostData<Comment, Comment>("/public/v2/posts/1/comments", It.IsAny<Comment>())).ReturnsAsync(comment);
            _mockApiClient.Setup(a => a.GetList<Todo>("/public/v2/users/1/todos")).ReturnsAsync(todos);

            //Act
            var result = await _service.ExternalService();

            //Assert
            Assert.True(Directory.Exists(Directory.GetCurrentDirectory() + "\\File\\"));
            Assert.True(File.Exists(result));
            File.Delete(result);
        }
        /// <summary>
        /// The function tests that an exception is thrown when calling the ExternalService method and
        /// the GetList method for users ReturnsAsync an exception.
        /// </summary>
        [Fact]
        public async Task GetList_Exception_ThrowsException()
        {
            List<User> users = new List<User>
           {
            new User {Id = 1,Name = "John Doe",Email = "johndoe@example.com",Gender = "Male",Status = "Active"  },
           };

            User user = new User
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Gender = "male",
                Status = "active"
            };

            //Creating Post of Users
            List<Post> posts = new List<Post>()
          {
            new Post(){  Id = 1,  Body = "This is the first post.",  Title = "First Post",  UserId = 1},
          };

            //Creating Comment for users
            List<Comment> comments = new List<Comment>
          {
            new Comment{ Id = 1, Email = "john@example.com", Body = "This is a great post!", Name = "John Doe", PostId = 1  },
          };
            //Creating Todo for user
            List<Todo> todos = new List<Todo>
         {
            new Todo { Id = 1, Title = "Complete project", UserId = 1, DueOn = new DateTime(2023, 04, 30) },
            new Todo { Id = 2, Title = "Finish presentation", UserId = 1, DueOn = new DateTime(2023, 05, 10) },
         };
            _mockApiClient.Setup(a => a.GetList<User>("/public/v2/users")).Throws(new Exception("Something went wrong")); ;
            _mockApiClient.Setup(a => a.GetList<Todo>("/public/v2/users/1/todos")).ReturnsAsync(todos);
            _mockApiClient.Setup(a => a.GetList<Post>("/public/v2/users/1/posts")).ReturnsAsync(posts);
            _mockApiClient.Setup(a => a.GetList<Comment>("/public/v2/posts/1/comments")).ReturnsAsync(comments);
            _mockApiClient.Setup(a => a.Get<User>("/public/v2/users/1")).ReturnsAsync(user);

            //Assert

            var exTask = Assert.ThrowsAsync<Exception>(async () => await _service.ExternalService());
            Exception ex = await exTask;
            Assert.Equal("Something went wrong", ex.Message);
        }

    }
}

