using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using ComplaintManagementSystem.Models;
using Microsoft.Extensions.Configuration;

namespace ComplaintManagementSystem.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly IConfiguration _configuration;

        // 🧩 Change between "Admin", "Staff", or "User" to test roles
        private string currentUserRole = "User";

        public ComplaintsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ===================== READ =====================
        public IActionResult Index()
        {
            List<Complaints> complaints = new List<Complaints>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Complaints ORDER BY DateCreated DESC", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        complaints.Add(new Complaints
                        {
                            ComplaintID = (int)reader["ComplaintID"],
                            UserID = (int)reader["UserID"],
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Status = reader["Status"].ToString(),
                            DateCreated = (System.DateTime)reader["DateCreated"]
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error loading complaints: " + ex.Message;
            }

            // Send current user role to the view
            ViewBag.UserRole = currentUserRole;

            return View(complaints);
        }

        // ===================== CREATE (Form Page) =====================
        public IActionResult Create()
        {
            return View();
        }

        // ===================== HttpGet ===================================
        [HttpGet]
        [Route("api/complaints")]
        public IActionResult GetComplaints()
        {
            List<Complaints> complaints = new List<Complaints>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Complaints ORDER BY DateCreated DESC", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        complaints.Add(new Complaints
                        {
                            ComplaintID = (int)reader["ComplaintID"],
                            UserID = (int)reader["UserID"],
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Status = reader["Status"].ToString(),
                            DateCreated = (System.DateTime)reader["DateCreated"]
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { message = "Error loading complaints", error = ex.Message });
            }

            return Ok(complaints); // ✅ Postman will display this as JSON
}
        // ===================== Change Role ============================

        [HttpGet]
        [Route("api/users/{id}/changerole")]
        public IActionResult ChangeUserRole(int id, [FromQuery] string newRole)
        {
            // Validate role input
            if (string.IsNullOrEmpty(newRole) ||
            !(newRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            newRole.Equals("Staff", StringComparison.OrdinalIgnoreCase) ||
            newRole.Equals("User", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Invalid role. Allowed roles: Admin, Staff, User." });
            }

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE Users SET Role = @Role WHERE UserID = @UserID", conn);
                    cmd.Parameters.AddWithValue("@Role", newRole);
                    cmd.Parameters.AddWithValue("@UserID", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        return NotFound(new { message = "User not found." });
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { message = "Database error.", error = ex.Message });
            }

            return Ok(new { message = $"User ID {id} role successfully changed to {newRole}." });

        }

        // ===================== CREATE (Handle Submission) =====================
        [HttpPost]
        public IActionResult Create(Complaints complaint)
        {
            // 💡 Ignore validation for Status field
            ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                return View(complaint);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Complaints (UserID, Title, Description, Status, DateCreated) VALUES (@UserID, @Title, @Description, 'Pending', GETDATE())",
                        conn);
                    cmd.Parameters.AddWithValue("@UserID", 3); // Example: Lily’s UserID
                    cmd.Parameters.AddWithValue("@Title", complaint.Title);
                    cmd.Parameters.AddWithValue("@Description", complaint.Description);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error saving complaint: " + ex.Message;
                return View(complaint);
            }

            return RedirectToAction("Index");
        }

        // ===================== EDIT (Form Page) =====================
        public IActionResult Edit(int id)
        {
            // Restrict edit access for non-admin/staff users
            if (currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Unauthorized("You do not have permission to edit complaints.");
            }

            Complaints complaint = new Complaints();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Complaints WHERE ComplaintID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        complaint.ComplaintID = (int)reader["ComplaintID"];
                        complaint.Title = reader["Title"].ToString();
                        complaint.Description = reader["Description"].ToString();
                        complaint.Status = reader["Status"].ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error loading complaint: " + ex.Message;
            }

            return View(complaint);
        }

        // ===================== EDIT (Handle Update) =====================
        [HttpPost]
        public IActionResult Edit(Complaints complaint)
        {
            // Restrict update access for non-admin/staff users
            if (currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Unauthorized("You do not have permission to update complaints.");
            }

            if (!ModelState.IsValid)
            {
                return View(complaint);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Complaints SET Title=@Title, Description=@Description, Status=@Status WHERE ComplaintID=@ComplaintID",
                        conn);
                    cmd.Parameters.AddWithValue("@Title", complaint.Title);
                    cmd.Parameters.AddWithValue("@Description", complaint.Description);
                    cmd.Parameters.AddWithValue("@Status", complaint.Status);
                    cmd.Parameters.AddWithValue("@ComplaintID", complaint.ComplaintID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error updating complaint: " + ex.Message;
                return View(complaint);
            }

            return RedirectToAction("Index");
        }

        // ===================== DELETE =====================
        public IActionResult Delete(int id)
        {
            // Restrict delete access for non-admin/staff users
            if (currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Unauthorized("You do not have permission to delete complaints.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Complaints WHERE ComplaintID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error deleting complaint: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
