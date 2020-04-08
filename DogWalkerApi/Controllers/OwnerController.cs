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
        public async Task<IActionResult> Get(
            [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id, o.DogOwnerName, o.DogOwnerAddress, o.NeighborhoodId, o.Phone ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += ", n.Id, n.NeighborhoodName AS Neighborhood ";
                    }
                    cmd.CommandText += "FROM DogOwner o";
                    if (include =="neighborhood")
                    {
                        cmd.CommandText += "LEFT JOIN Neighborhood n ON o.neighborhoodId = n.Id";
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Owners> owners = new List<Owners>();

                    Owners owner = null;

                    while (reader.Read())
                    {
                        if (include == "neighborhood")
                        {
                            owner = new Owners
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                DogOwnerName = reader.GetString(reader.GetOrdinal("DogOwnerName")),
                                DogOwnerAddress = reader.GetString(reader.GetOrdinal("DogOwnerAddress")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhoods()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    NeighborhoodName = reader.GetString(reader.GetOrdinal("Neighborhood"))
                                },
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            };
                        }
                        else
                        {
                            owner = new Owners
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                DogOwnerName = reader.GetString(reader.GetOrdinal("DogOwnerName")),
                                DogOwnerAddress = reader.GetString(reader.GetOrdinal("DogOwnerAddress")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                            };
                        }
                        owners.Add(owner);
                    }
                    reader.Close();

                    return Ok(owners);
                }
            }
        }

        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromRoute] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id, o.DogOwnerName, o.DogOwnerAddress, o.NeighborhoodId, o.Phone ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += ", n.Id, n.NeighborhoodName AS Neighborhood ";
                    }
                    cmd.CommandText += "FROM DogOwner o";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += "LEFT JOIN Neighborhood n ON o.neighborhoodId = n.Id";
                    }
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owners owner = null;

                    if (include == "neighborhood")
                    {
                        while (reader.Read())
                        {
                            if (owner == null)
                            {
                                owner = new Owners
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    DogOwnerName = reader.GetString(reader.GetOrdinal("DogOwnerName")),
                                    DogOwnerAddress = reader.GetString(reader.GetOrdinal("DogOwnerAddress")),
                                    NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Neighborhood = new Neighborhoods()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                        NeighborhoodName = reader.GetString(reader.GetOrdinal("Neighborhood"))
                                    },
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    DogsList = new List<Dogs>()
                                };
                            }
                            owner.DogsList.Add(new Dogs()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                DogName = reader.GetString(reader.GetOrdinal("DogName")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                DogOwnerId = reader.GetInt32(reader.GetOrdinal("DogOwnerId"))
                            });
                        }
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