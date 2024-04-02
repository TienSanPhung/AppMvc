namespace AppMvc.Models;

public class Sommernote
{
    public Sommernote(string _IdEditor,bool _LoadLibrary = true)
    {
        IdEditor = _IdEditor;
        LoadLibrary = _LoadLibrary;
    }

    public string IdEditor { get; set; }
    public bool LoadLibrary {set;get;}
     public int Height { get; set; } = 120;
     public string ToolBar  { get; set; } = @"
       [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            ['fontname', ['fontname']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'video', 'elfinder']],
            ['view', ['fullscreen', 'codeview', 'help']],
        ]
    ";
    
    
}
