﻿namespace Concert.Business.Models.Domain
{
    public class SongRequestDto
    {
        public int Id { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Language { get; set; }
    }
}