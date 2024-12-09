using Concert.Business.Models.Domain;

namespace Concert.DataAccess.InMemory.Repositories
{
    public class SongRequestsIMRepository : ISongRequestsRepository
    {
        private static List<SongRequest> _songRequests = new List<SongRequest>()
        {
            new SongRequest()
            {
                Id = 1,
                Artist = "Ace of base",
                Title = "All that she wants",
                Genre = "Reggae",
                Language = "English",
            },
            new SongRequest()
            {
                Id = 2,
                Artist = "And One",
                Title = "Military fashion show",
                Genre = "EBM",
                Language = "English"
            },
            new SongRequest()
            {
                Id = 3,
                Artist = "Ascendant Vierge",
                Title = "Influenceur",
                Genre = "EDM",
                Language = "French"
            },
            new SongRequest()
            {
                Id = 4,
                Artist = "Boys",
                Title = "Szalona",
                Genre = "Disco polo",
                Language = "Polish"
            },
            new SongRequest()
            {
                Id = 5,
                Artist = "Charles Aznavour",
                Title = "For me Formidable",
                Genre = "Chanson française",
                Language = "-"
            }
        };

        public List<SongRequest> GetAll() => _songRequests;

        public SongRequest? GetById(int id)
        {
            var songRequest = _songRequests.Find(s => s.Id == id);
            if (songRequest is not null)
            {
                // Instead of returning the song request because otherwise any changes
                // in the song request variable would change the object in the list
                return new SongRequest
                {
                    Id = songRequest.Id,
                    Artist = songRequest.Artist,
                    Title = songRequest.Title,
                    Genre = songRequest.Genre,
                    Language = songRequest.Language
                };
            }

            return null;
        }

        public void Update(int id, SongRequest songRequest)
        {
            if (id != songRequest.Id)
            {
                return;
            }

            var songRequestToUpdate = _songRequests.Find(s => s.Id == id);
            if (songRequestToUpdate is not null)
            {
                songRequestToUpdate.Artist = songRequest.Artist;
                songRequestToUpdate.Title = songRequest.Title;
                songRequestToUpdate.Genre = songRequest.Genre;
                songRequestToUpdate.Language = songRequest.Language;
            }
        }
    }
}