using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seer.Infrastructure.Models;

[Table("squire_accounts")]
public class SquireAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string MatchName { get; set; }
    [ForeignKey("GroupId")]
    public int GroupId { get; set; }
}