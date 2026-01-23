using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp8.Entities;

public class EntryTag
{
    public int JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}
