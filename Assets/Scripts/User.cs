namespace FriendsSystem
{
    [System.Serializable]
    public abstract class BaseUser
    {
        public string UserName;
    }

    [System.Serializable]
    public class User : BaseUser
    {
        public bool IsOnline;
        public User(string userName)
        {
            UserName = userName;
            IsOnline = false;
        }
    }
}