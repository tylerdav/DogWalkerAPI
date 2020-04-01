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
    public class DogsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogsController(IConfiguration config)
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
                    cmd.CommandText = "SELECT Id, DogName, DogOwnerId, Breed, Notes FROM Dog";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Dogs> dogs = new List<Dogs>();

                    while (reader.Read())
                    {
                        Dogs dog = new Dogs
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            DogOwnerId = reader.GetInt32(reader.GetOrdinal("DogOwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes"))
                        };

                        dogs.Add(dog);
                    }
                    reader.Close();

                    return Ok(dogs);
                }
            }
        }

        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, DogName, DogOwnerId, Breed, Notes
                        FROM 
                            Dog
                        WHERE 
                            Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dogs dog = null;

                    if (reader.Read())
                    {
                        dog = new Dogs
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            DogOwnerId = reader.GetInt32(reader.GetOrdinal("DogOwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes"))
                        };
                    }
                    reader.Close();

                    return Ok(dog);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Dogs dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Dog (DogName, DogOwnerId, Breed, Notes)
                                        OUTPUT INSERTED.Id 
                                        VALUES (@dogName, @dogOwnerId, @breed, @notes)";

                    cmd.Parameters.Add(new SqlParameter("@id", dog.Id));
                    cmd.Parameters.Add(new SqlParameter("@dogName", dog.DogName));
                    cmd.Parameters.Add(new SqlParameter("@DogOwnerId", dog.DogOwnerId));
                    cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                    cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));

                    int newId = (int)cmd.ExecuteScalar();
                    dog.Id = newId;
                    return CreatedAtRoute("GetDog", new { id = newId }, dog);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Dogs dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        UPDATE Dog
                        Set DogName = @dogName, DogOwnerId = @dogOwnerid, Breed = @breed, Notes = @notes
                        WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", dog.Id));
                        cmd.Parameters.Add(new SqlParameter("@dogName", dog.DogName));
                        cmd.Parameters.Add(new SqlParameter("@dogOwnerId", dog.DogOwnerId));
                        cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));

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
                if (!DogExists(id))
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
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
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
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT
                        Id, DogName, DogOwnerId, Breed, Notes
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