using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using DogWalkerApi.Models;

namespace DogWalkerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OwnersController(IConfiguration config)
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
                    cmd.CommandText = "SELECT Id, DogOwnerName, DogOwnerAddress, NeighborhoodId, Phone FROM DogOwner";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Owners> owners = new List<Owners>();

                    while (reader.Read())
                    {
                        Owners owner = new Owners
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogOwnerName = reader.GetString(reader.GetOrdinal("DogOwnerName")),
                            DogOwnerAddress = reader.GetString(reader.GetOrdinal("DogOwnerAddress")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };

                        owners.Add(owner);
                    }
                    reader.Close();

                    return Ok(owners);
                }
            }
        }

        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, DogOwnerName, DogOwnerAddress, NeighborhoodId, Phone
                        FROM 
                            Owner
                        WHERE 
                            Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owners owner = null;

                    if (reader.Read())
                    {
                        owner = new Owners
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogOwnerName = reader.GetString(reader.GetOrdinal("DogOwnerName")),
                            DogOwnerAddress = reader.GetString(reader.GetOrdinal("DogOwnerAddress")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };
                    }
                    reader.Close();

                    return Ok(owner);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owners owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Owner (DogOwnerName, DogOwnerAddress, NeighborhoodId, Phone)
                                        OUTPUT INSERTED.Id 
                                        VALUES (@dogOwnerName, @dogOwnerAddress, @neighborhoodId, @phone)";

                    cmd.Parameters.Add(new SqlParameter("@id", owner.Id));
                    cmd.Parameters.Add(new SqlParameter("@dogOwnerName", owner.DogOwnerName));
                    cmd.Parameters.Add(new SqlParameter("@dogOwnerAddress", owner.DogOwnerAddress));
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));

                    int newId = (int)cmd.ExecuteScalar();
                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owners owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        UPDATE Owner
                        Set DogOwnerName = @dogOwnerName, DogOwnerAddress = @dogOwnerAddress, NeighborhoodId = @neighborhoodId, Phone = @phone
                        WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", owner.Id));
                        cmd.Parameters.Add(new SqlParameter("@dogOwnerName", owner.DogOwnerName));
                        cmd.Parameters.Add(new SqlParameter("@dogOwnerAddress", owner.DogOwnerAddress));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));

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
                if (!OwnerExists(id))
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
                        cmd.CommandText = @"DELETE FROM Owner WHERE Id = @id";
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
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT
                        Id, Name, OwnerId, Breed, Notes
                        FROM
                            Dog
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