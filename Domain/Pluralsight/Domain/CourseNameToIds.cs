using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class CourseNameToIds
{
    private readonly IRestHelper restHelper;

    public CourseNameToIds(IRestHelper restHelper)
    {
        this.restHelper = restHelper;
    }

    public async Task<NameToIdsResult> Execute(IList<string> courseNames)
    {
        string resource = "/library/courses/nametoids?courses=" + string.Join(",", courseNames);
        RestResponse<NameToIdsResult> restResponse = await restHelper.BasicGet<NameToIdsResult>(resource);
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
            return restResponse.Data;
        }
        return null;
    }
}
