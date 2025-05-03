using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Constants
{
    public class ApiRoutes
    {
        public const string BaseUrl = "https://localhost:6969";

        // Auth endpoints
        public const string AuthLogin = "/api/auth/login";
        public const string AuthSendCodeEmail = "/api/auth/sendCodeEmail";
        public const string AuthVerifyCode = "/api/auth/verifiCode";

        // User endpoints
        public const string UserNewUser = "/api/user/newUser";
        public const string UserEditUser = "/api/user/editUser";

        // Comment endpoints
        public const string CommentCreate = "/api/comment/comment";
        public const string CommentGetBySongId = "/api/comment/getComment/{song_id}/comments";
        public const string CommentGetById = "/api/comment/getComment/comment/{id}";
        public const string CommentDelete = "/api/comment/delete/{id}";

        // Notification endpoints
        public const string NotificationCreate = "/api/notifications/createNotification";
        public const string NotificationGetById = "/api/notifications/{id}/notification";
        public const string NotificationGetByUserId = "/api/notifications/getNotifications/{userId}";
        public const string NotificationDelete = "/api/notifications/delete/{id}";
        public const string NotificationMarkAsRead = "/api/notifications/notification/{id}/read";
    }
}
