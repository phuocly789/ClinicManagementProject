// File: UserRegisterDto.cs

using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Application.DTOs.Auth
{
    // File: CreatePatientByReceptionistDto.cs
    public class CreatePatientByReceptionistDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^0\d{9}$")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
    }
}
