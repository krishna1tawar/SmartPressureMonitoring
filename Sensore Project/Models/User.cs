namespace Sensore_Project.Models
{
    /// <summary>
    /// Represents a user in the system (Patient, Clinician, or Admin).
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// User role: Patient, Clinician, or Admin.
        /// </summary>
        public string Role { get; set; } = "Patient";
    }
}