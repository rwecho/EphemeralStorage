using FileTtl.BackgroundJobs;
using FileTtl.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FileTtl.Pages;

public class IndexModel : PageModel
{
    private  ILogger<IndexModel> Logger { get; }
    private  FileManager FileManager { get; }

    public IndexModel(ILogger<IndexModel> logger, FileManager fileManager)
    {
        Logger = logger;
        this.FileManager = fileManager;
    }

    [BindProperty]
    public IFormFile? SelectedFile {  get; set; }


    [BindProperty(SupportsGet = true)]
    public string? Hash { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FileName { get; set; }

    public ActionResult OnGet()
    {
        return Page();
    }

    public async Task<ActionResult> OnPost()
    {
        if (SelectedFile == null)
        {
            return Page(); ;
        }

        var fileItem = await this.FileManager.UploadAsync(this.SelectedFile, default);

        return this.RedirectToPage("/Index", new { hash = fileItem.Hash, fileName = fileItem.FileName });
    }
}
