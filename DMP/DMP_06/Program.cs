using DMP_06.Services;

namespace DMP_06;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // MVC
        builder.Services.AddControllersWithViews();

        // ONNX Predictor (singleton)
        builder.Services.AddSingleton(sp =>
        {
            var baseDir = AppContext.BaseDirectory;
            var regPath = Path.Combine(baseDir, "ModelsOnnx", "rf_reg_model.onnx");
            var clfPath = Path.Combine(baseDir, "ModelsOnnx", "rf_clf_model.onnx");

            return new OnnxPredictor(regPath, clfPath);
        });

        // Background warmup
        builder.Services.AddHostedService<ModelWarmupService>();

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouting();
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("not ready"))
            {
                context.Response.StatusCode = 503;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    status = "loading",
                    message = "Models are loading"
                });
            }
        });

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}