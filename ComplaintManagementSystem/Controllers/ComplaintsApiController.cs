using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using ComplaintManagementSystem.Models;
using Microsoft.Extensions.Configuration;

namespace ComplaintManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintsApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ComplaintsApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ===================== GET ALL =====================
        [HttpGet("GetAllComplaints")]
        public IActionResult GetAllComplaints()
        {
            List<Complaints> complaints = new List<Complaints>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

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

            return Ok(complaints);
        }

        // ===================== GET BY ID =====================
        [HttpGet("GetComplaintById/{id}")]
        public IActionResult GetComplaintById(int id)
        {
            Complaints complaint = null;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Complaints WHERE ComplaintID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    complaint = new Complaints
                    {
                        ComplaintID = (int)reader["ComplaintID"],
                        UserID = (int)reader["UserID"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        Status = reader["Status"].ToString(),
                        DateCreated = (System.DateTime)reader["DateCreated"]
                    };
                }
            }

            if (complaint == null)
                return NotFound(new { message = "Complaint not found" });

            return Ok(complaint);
        }

        // ===================== ADD (POST) =====================
        [HttpPost("AddComplaint")]
        public IActionResult AddComplaint([FromBody] Complaints complaint)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Complaints (UserID, Title, Description, Status, DateCreated) VALUES (@UserID, @Title, @Description, 'Pending', GETDATE())",
                    conn);
                cmd.Parameters.AddWithValue("@UserID", complaint.UserID);
                cmd.Parameters.AddWithValue("@Title", complaint.Title);
                cmd.Parameters.AddWithValue("@Description", complaint.Description);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Complaint added successfully" });
        }

        // ===================== UPDATE (PUT) =====================
        [HttpPut("UpdateComplaint/{id}")]
        public IActionResult UpdateComplaint(int id, [FromBody] Complaints complaint)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Complaints SET Title=@Title, Description=@Description, Status=@Status WHERE ComplaintID=@ComplaintID",
                    conn);
                cmd.Parameters.AddWithValue("@Title", complaint.Title);
                cmd.Parameters.AddWithValue("@Description", complaint.Description);
                cmd.Parameters.AddWithValue("@Status", complaint.Status);
                cmd.Parameters.AddWithValue("@ComplaintID", id);
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound(new { message = "Complaint not found" });
            }

            return Ok(new { message = "Complaint updated successfully" });
        }

        // ===================== DELETE =====================
        [HttpDelete("DeleteComplaint/{id}")]
        public IActionResult DeleteComplaint(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Complaints WHERE ComplaintID=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound(new { message = "Complaint not found" });
            }

            return Ok(new { message = "Complaint deleted successfully" });
        }
    }
}
