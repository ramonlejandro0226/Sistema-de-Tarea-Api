﻿namespace Sistema_De_gestion_De_Tarea.DTOs.TaskDTO
{
    public class TaskPutDTO
    {
        public Guid id { get; set; }

        public string Description { get; set; } = null!;
        public StatusDTO Status { get; set; } = null!;
    }
}
