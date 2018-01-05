using System.Collections.Generic;

namespace MpcNET.Types
{
    public interface IMpdFile : IMpdFilePath
    {
        int Time { get; }
        string Album { get; } 
        string Artist { get; }
        string Title { get; }
        string Track { get; }
        string Name { get; }
        string Genre { get; }
        string Date { get; }
        string Composer { get; }
        string Performer { get; }
        string Comment { get; }
        int Disc { get; }
        int Pos { get; }
        int Id { get; }
        IReadOnlyDictionary<string, string> UnknownMetadata { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Time"/> property set.
        /// </summary>
        bool HasTime { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Album"/> property set.
        /// </summary>
        bool HasAlbum { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Artist"/> property set.
        /// </summary>
        bool HasArtist { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Title"/> property set.
        /// </summary>
        bool HasTitle { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Track"/> property set.
        /// </summary>
        bool HasTrack { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Name"/> property set.
        /// </summary>
        bool HasName { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Genre"/> property set.
        /// </summary>
        bool HasGenre { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Date"/> property set.
        /// </summary>
        bool HasDate { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Composer"/> property set.
        /// </summary>
        bool HasComposer { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Performer"/> property set.
        /// </summary>
        bool HasPerformer { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Comment"/> property set.
        /// </summary>
        bool HasComment { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Disc"/> property set.
        /// </summary>
        bool HasDisc { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Pos"/> property set.
        /// </summary>
        bool HasPos { get; }

        /// <summary>
        /// If the MpdFile has the <see cref="Id"/> property set.
        /// </summary>
        bool HasId { get; }
    }
}