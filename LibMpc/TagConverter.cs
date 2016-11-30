using System;

namespace LibMpc
{
    public static class TagConverter
    {
        private const string TagAny = "any";
        private const string TagFilename = "filename";
        private const string TagArtist = "artist";
        private const string TagAlbum = "album";
        private const string TagTitle = "title";
        private const string TagTrack = "track";
        private const string TagName = "name";
        private const string TagGenre = "genre";
        private const string TagDate = "date";
        private const string TagComposer = "composer";
        private const string TagPerformer = "performer";
        private const string TagComment = "comment";
        private const string TagDisc = "disc";

        public static string ToTag(ScopeSpecifier scopeSpecifier)
        {
            switch (scopeSpecifier)
            {
                default:
                    throw new ArgumentException("scopeSpecifier");
                case ScopeSpecifier.Any:
                    return TagAny;
                case ScopeSpecifier.Filename:
                    return TagFilename;
                case ScopeSpecifier.Artist:
                    return TagArtist;
                case ScopeSpecifier.Album:
                    return TagAlbum;
                case ScopeSpecifier.Title:
                    return TagTitle;
                case ScopeSpecifier.Track:
                    return TagTrack;
                case ScopeSpecifier.Name:
                    return TagName;
                case ScopeSpecifier.Genre:
                    return TagGenre;
                case ScopeSpecifier.Date:
                    return TagDate;
                case ScopeSpecifier.Composer:
                    return TagComposer;
                case ScopeSpecifier.Performer:
                    return TagPerformer;
                case ScopeSpecifier.Comment:
                    return TagComment;
                case ScopeSpecifier.Disc:
                    return TagDisc;
            }
        }
    }
}