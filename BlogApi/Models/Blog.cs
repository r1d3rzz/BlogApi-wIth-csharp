using System;
using System.Collections.Generic;

namespace BlogApi.Models;

public partial class Blog
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    //for rs
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
