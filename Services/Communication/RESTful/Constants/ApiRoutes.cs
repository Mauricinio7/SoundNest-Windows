using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Constants
{
    public class ApiRoutes
    {
        //public const string BaseUrl = "http://localhost:6970/";
        //public const string BaseUrl = "https://allowing-walrus-classic.ngrok-free.app/restful/";
        //public const string BaseUrl = "https://allowing-walrus-classic.ngrok-free.app/restful/";
        public const string BaseUrl = "http://100.65.158.22/";

        // Auth endpoints:
        public const string AuthValidateJWT = "api/auth/validateJWT";
        public const string AuthLogin = "api/auth/login";
        public const string AuthSendCodeEmail = "api/auth/sendCodeEmail";
        public const string AuthVerifyCode = "api/auth/verifiCode";

        // User endpoints
        public const string UserNewUser = "api/user/newUser";
        public const string UserEditUser = "api/user/editUser";
        public const string UserEditPassword = "api/user/editUserPassword";
        public const string UserGetAditionalInformation = "api/user/get/aditionalInfo";

        // Comment endpoints
        public const string CommentRespondComment = "api/comment/{commentId}/respondComment";
        public const string CommentCreate = "api/comment/createComment";
        public const string CommentGetBySongId = "api/comment/getComment/{song_id}/song";
        public const string CommentGetById = "api/comment/{id}/all";
        public const string CommentDelete = "api/comment/delete/{id}";
        public const string CommentGetRepliesByCommentId = "api/comment/responses/flat/{id}";

        // Notification endpoints
        public const string NotificationCreate = "api/notifications/createNotification";
        public const string NotificationGetById = "api/notifications/{id}/notification";
        public const string NotificationGetByUserId = "api/notifications/getNotifications/{userId}";
        public const string NotificationDelete = "api/notifications/delete/{id}";
        public const string NotificationMarkAsRead = "api/notifications/notification/{id}/read";

        //Songs endpoints
        public const string SongsDeleteSong = "api/songs/{idsong}/delete";
        public const string SongsPatchSongImage = "api/songs/{idsong}/image";
        public const string SongsGetLatestSongsByUserId = "api/songs/user/{idAppUser}/lastest";
        public const string SongsGetByUserId = "api/songs/user/{idAppUser}";
        public const string SongSearchBase = "api/songs/search";
        public const string SongsGetMostPopulars = "api/songs/{amount}/popular/{year}/{month}";
        public const string SongsGetMostRecent = "api/songs/{amount}/recent";
        public const string SongsGetRandom = "api/songs/{amount}/random";
        public const string SongGetById = "api/songs/{idsong}/song";
        public const string SongGetGenres = "api/songs/genres";
        public const string SongGetExtensions = "songs/extensions";
        public const string SongUploadImage = "api/songs/{idsong}/base64/image";
        public const string SongGetLastestUserSong = "api/songs/user/{idAppUser}/lastest";

        //Playlist endpoints
        public const string PlaylistCleanDeletedSongs = "api/playlist/list/{idPlaylist}/clean";
        public const string PlaylistPatchEditPlaylist = "api/playlist/edit/{idPlaylist}";
        public const string PlaylistGetById = "api/playlist/one/{idPlaylist}";
        public const string PlaylistGetByUserId = "api/playlist/{idUser}/user";
        public const string PlaylistPatchRemoveSong = "api/playlist/{idsong}/{idPlaylist}/remove";
        public const string PlaylistPatchAddSong = "api/playlist/{idsong}/{idPlaylist}/add";
        public const string PlaylistDelete = "api/playlist/{idPlaylist}/delete/";
        public const string PlaylistPutNewPlaylist = "api/playlist/base64/upload";

        //Visualizaations
        public const string AddVisitToSong = "api/visit/{idSong}/increment";
        public const string GetTopSongsByUser = "api/visit/user/{idUser}/top-songs";
        public const string GetTopSongsGlobaly = "api/visit/global/top-songs";
        public const string GetTopGenresGlobaly = "api/visit/global/top-genres";
    }
}
