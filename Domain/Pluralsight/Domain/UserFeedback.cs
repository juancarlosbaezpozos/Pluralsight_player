using Pluralsight.Domain.Authentication;

namespace Pluralsight.Domain;

public class UserFeedback
{
	private IRestHelper restHelper;

	public UserFeedback(IRestHelper restHelper)
	{
		this.restHelper = restHelper;
	}

	public async void SendFeedback(FeedbackDto feedback, AuthenticationToken authenticationToken)
	{
		await restHelper.AuthenticatedPost<string, FeedbackDto>("user/feedback/comment", feedback);
	}
}
