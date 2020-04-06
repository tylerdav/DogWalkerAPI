using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using DogWalkerApi.Models;
using Microsoft.AspNetCore.Http;

namespace DogWalkerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalkersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WalkersController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, WalkerName, NeighborhoodId FROM Walker";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Walkers> walker = new List<Walkers>();

                    while (reader.Read())
                    {
                        Walkers walkers = new Walkers
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            WalkerName = reader.GetString(reader.GetOrdinal("WalkerName")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))

                        };

                        walker.Add(walkers);
                    }
                    reader.Close();

                    return Ok(walker);
                }
            }
        }

        [HttpGet("{id}", Name = "GetWalker")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT wr.Id, wr.WalkerName, wr.NeighborhoodId";
                    if (include == "walks")
                    {
                        cmd.CommandText += ", ws.Id AS WalksId, ws.WalkDate, ws.Duration, ws.DogId, ws.WalkerId ";
                    }
                    cmd.CommandText += "FROM Walker wr ";
                    
                    if(include == "walks")
                    {
                        cmd.CommandText += "LEFT JOIN walk ws ON wr.Id = ws.WalkerId ";
                    }
                    cmd.CommandText += "WHERE wr.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walkers walker = null;

                    while (reader.Read())
                    {
                        if (walker == null)
                        {
                            walker = new Walkers
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                WalkerName = reader.GetString(reader.GetOrdinal("WalkerName")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                            };


                        }

                        if (include == "walks")
                        {
                            walker.Walk.Add(new Walks()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("WalksId")),
                                WalkDate = reader.GetDateTime(reader.GetOrdinal("WalkDate")),
                                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                                WalkerId = reader.GetInt32(reader.GetOrdinal("WalkerId")),
                                DogId = reader.GetInt32(reader.GetOrdinal("DogId"))
                            });
                        }
                    }
                    reader.Close();

                    return Ok(walker);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Walkers walker)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Walker (WalkerName, NeighborhoodId)
                                        OUTPUT INSERTED.Id 
                                        VALUES (@walkerName, @neighborhoodId)";

                    cmd.Parameters.Add(new SqlParameter("@id", walker.Id));
                    cmd.Parameters.Add(new SqlParameter("@walkerName", walker.WalkerName));
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));

                    int newId = (int)cmd.ExecuteScalar();
                    walker.Id = newId;
                    return CreatedAtRoute("GetWalker", new { id = newId }, walker);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walkers walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        UPDATE Walker
                        Set WalkerName = @walkerName, NeighborhoodId = @neighborhoodId
                        WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", walker.Id));
                        cmd.Parameters.Add(new SqlParameter("@walkerName", walker.WalkerName));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        else
                        {
                            throw new Exception("No rows affected");
                        }
                    }
                }
            }

            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Walker WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }

            catch (Exception)
            {
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool WalkerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT
                        Id, WalkerName, NeighborhoodId
                        FROM
                            Walker
                        WHERE
                            Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}