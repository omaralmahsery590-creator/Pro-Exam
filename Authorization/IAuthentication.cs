namespace pro_exam.Authorization
{
    public interface IAuthentication<T>// where T : class
    {
        string GetJsonWebToken(T entity);// Generate a JWT token based on the provided entity (e.g., user information)
    }
}

