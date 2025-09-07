using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChequioTask.Models
{
    // Simple status lifecycle for cheques
    public enum ChequeStatus
    {
        [Display(Name = "Draft")] Draft = 0,
        [Display(Name = "Issued")] Issued = 1,
        [Display(Name = "Cleared")] Cleared = 2,
        [Display(Name = "Bounced")] Bounced = 3,
        [Display(Name = "Voided")] Voided = 4
    }

    [Index(nameof(Number), IsUnique = true)] // enforce unique cheque number
    public class Cheque
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Number { get; set; } = default!; // e.g., bank serial / cheque number

        [Required, StringLength(120)]
        public string PayeeName { get; set; } = default!; // person/company to pay

        [Precision(18, 2)]
        [Range(0.01, 999999999999.99)]
        public decimal Amount { get; set; } // currency amount

        [Required, StringLength(3)]
        public string Currency { get; set; } = "JOD"; // ISO-like (e.g., JOD, USD)

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today;

        public ChequeStatus Status { get; set; } = ChequeStatus.Draft;

        [StringLength(500)]
        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
