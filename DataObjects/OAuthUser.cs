namespace UI.Logic.OAuth
{
    /// <summary>
    /// Reprsents the OAuth Provider's user personal details
    /// </summary>
    public class OAuthUser
    {
        public long id { get; set; }

        /// <summary>
        /// oauth provider's user name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// oauth provider's nickname
        /// </summary>
        public string screen_name { get; set; }

        /// <summary>
        /// profile avatar
        /// </summary>
        public string profile_image_url { get; set; }

    }
}