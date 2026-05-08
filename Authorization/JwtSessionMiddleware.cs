namespace pro_exam.Authorization;
public class JwtSessionMiddleware// Middleware class to handle JWT token in session and add it to the request headers
{
    private readonly RequestDelegate _next;

    public JwtSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Session.GetString("token");// Retrieve the JWT token from the session
        if (!string.IsNullOrEmpty(token))// Check if the token is not null or empty
        {
            context.Request.Headers.Add("Authorization", "Bearer " + token);// Add the token to the request headers with the "Bearer " prefix   
        }
        await _next(context);// Call the next middleware in the pipeline
    }
}

