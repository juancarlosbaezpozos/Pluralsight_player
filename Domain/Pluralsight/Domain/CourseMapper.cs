using System.Collections.Generic;
using System.Linq;

namespace Pluralsight.Domain;

public static class CourseMapper
{
    public static CourseDetail ToCourseDetail(CourseDetailDao dao)
    {
        if (dao == null)
        {
            return null;
        }
        CourseDetail courseDetail = ToCourseDetail(dao.Header);
        if (courseDetail != null)
        {
            courseDetail.Modules = dao.Modules?.Select(ToModule).ToList();
            courseDetail.AudienceTags = dao.AudienceTags;
            courseDetail.Description = dao.Description;
        }
        return courseDetail;
    }

    private static Module ToModule(ModuleDao dao)
    {
        return new Module
        {
            Name = dao.Id,
            DurationInMilliseconds = dao.DurationInMilliseconds,
            Description = dao.Description,
            Clips = dao.Clips?.Select(ToClip).ToList(),
            Title = dao.Title,
            AuthorHandle = dao.AuthorHandle
        };
    }

    private static Clip ToClip(ClipDao dao)
    {
        return new Clip
        {
            DurationInMilliseconds = dao.DurationInMilliseconds,
            Name = dao.Id,
            Title = dao.Title,
            Index = dao.Index,
            SupportsStandard = dao.SupportsStandard,
            SupportsWidescreen = dao.SupportsWidescreen
        };
    }

    public static CourseDetail ToCourseDetail(CourseHeaderDao dao)
    {
        if (dao == null)
        {
            return null;
        }
        CourseDetail courseDetail = new CourseDetail
        {
            Modules = new List<Module>(),
            Name = dao.Id,
            UrlSlug = dao.Name,
            AudienceTags = new List<Tag>(),
            Authors = new List<Author>(),
            Title = dao.Title,
            ReleaseDate = dao.ReleaseDate,
            UpdatedDate = dao.UpdatedDate,
            Level = dao.Level,
            Color = dao.Color,
            ImageUrl = dao.ImageUrl,
            DefaultImageUrl = dao.DefaultImageUrl,
            AverageRating = dao.AverageRating,
            HasTranscript = dao.HasTranscript,
            ShortDescription = dao.ShortDescription,
            DurationInMilliseconds = dao.DurationInMilliseconds
        };
        if (dao.Authors != null)
        {
            foreach (AuthorHeaderDao author in dao.Authors)
            {
                courseDetail.Authors.Add(ToAuthor(author));
            }
        }
        return courseDetail;
    }

    public static SearchResults ToSearchResult(SearchResultsDao data)
    {
        SearchResults searchResults = new SearchResults
        {
            Collection = new List<SearchHit>(),
            Pagination = data.Pagination,
            TotalResults = data.TotalResults
        };
        foreach (CourseHeaderDao item in data.Collection)
        {
            searchResults.Collection.Add(new SearchHit
            {
                Course = ToCourseDetail(item),
                Type = "course"
            });
        }
        return searchResults;
    }

    public static Author ToAuthor(AuthorHeaderDao dao)
    {
        if (dao == null)
        {
            return null;
        }
        return new Author
        {
            Handle = dao.Id,
            FullName = dao.FullName,
            SmallImageUrl = dao.ImageUrl
        };
    }
}
