using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Constants
{
    public class ApiRoutes
    {
        public const string BaseUrl = "http://100.65.158.22/restful/";
        //public const string BaseUrl = "https://allowing-walrus-classic.ngrok-free.app/restful/";

        // Auth endpointshttps:
        public const string AuthLogin = "api/auth/login";
        public const string AuthSendCodeEmail = "api/auth/sendCodeEmail";
        public const string AuthVerifyCode = "api/auth/verifiCode";

        // User endpoints
        public const string UserNewUser = "api/user/newUser";
        public const string UserEditUser = "api/user/editUser";

        public const string UserValidateJWT = "api/user/validateJWT";

        // Comment endpoints
        public const string CommentCreate = "api/comment/createComment";
        public const string CommentGetBySongId = "getComment/{song_id}/comments";

        //public const string CommentGetBySongId = "api/comment/getComment/{song_id}/comments";
        public const string CommentGetById = "api/comment/getComment/comment/{id}";
        public const string CommentDelete = "api/comment/delete/{id}";

        // Notification endpoints
        public const string NotificationCreate = "api/notifications/createNotification";
        public const string NotificationGetById = "api/notifications/{id}/notification";
        public const string NotificationGetByUserId = "api/notifications/getNotifications/{userId}";
        public const string NotificationDelete = "api/notifications/delete/{id}";
        public const string NotificationMarkAsRead = "api/notifications/notification/{id}/read";

        //Songs endpoints
        public const string SongsGetMostPopulars = "api/songs/{amount}/popular/:year/:month";
        public const string SongsGetMostRecent = "/api/songs/{amount}/recent";
        public const string SongsGetRandom = "api/songs/{amount}/random";
        public const string SongGetById = "api/songs/{idsong}/song";
        public const string SongGetGenres = "api/songs/genres";
        public const string SongGetExtensions = "songs/extensions";
        public const string SongSearchBase = "api/songs/search?";

    }
}
