using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests;

public class PostCampfire : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public PostCampfire(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		string username = _dataAccess.RetrieveUsernameFromToken(authToken!);
		Guid cardId = new(HttpRequestUtility.ExtractJsonPayload(request).Trim('\"'));

		return !_dataAccess.DoesCardBelongToUser(cardId, username)
			|| _dataAccess.IsCardInUserDeck(cardId, username)
			|| _dataAccess.IsCardEngagedInTrade(cardId)
			? string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostCampfire_403)
			: ExecuteCardChange(cardId);
	}

	private string ExecuteCardChange(Guid cardId)
	{
		if (DidUpgradeSucceed())
		{
			_dataAccess.CampfireUpgrade(cardId);
			return string.Format(Text.HttpResponse_200_OK, Text.Description_PostCampfire_200_Success);
		}

		return !_dataAccess.CampfireCardLoss(cardId)
			? Text.HttpResponse_500_InternalServerError
			: string.Format(Text.HttpResponse_200_OK, Text.Description_PostCampfire_200_Fail);
	}

	private static bool DidUpgradeSucceed()
	{
		Random random = new();
		float randomNumber = (float)random.NextDouble();

		return randomNumber < Constants.CampfireOdds;
	}
}
