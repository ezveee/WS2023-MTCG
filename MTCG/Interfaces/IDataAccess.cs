using MTCG.Database.Schemas;

namespace MTCG.Interfaces;

public interface IDataAccess
{
	void CloseConnection();
	void DbSetup();
	void UpdateUserStats(string winner, string loser);
	string InsertUserData(string username, UserData userData);
	string ConfigureDeck(List<Guid> cardIds, string authToken);
	bool CreateDbUser(UserCredentials user);
	string AquirePackage(string authToken);
	string CreateTrade(TradingDeal trade, string username);
	void CreateSession(UserCredentials user, out string authToken);
	int DoUserAndPasswordExist(UserCredentials user);
	string CreatePackage(List<Database.Schemas.Card> package, string authToken);
	bool DoCardsAlreadyExist(List<Database.Schemas.Card> package);
	bool IsAdmin(string authToken);
	void InsertIntoCardsTable(List<Database.Schemas.Card> package);
	UserStats? RetrieveUserStats(string username);
	string RetrieveScoreboard(string username);
	string DeleteTrade(Guid tradeId, string username);
	string ExecuteTrade(Guid tradeId, Guid offeredCardId, string username);
	Tuple<string, float> GetCardRequirements(Guid cardId);
	Tuple<string, float> GetTradeRequirements(Guid tradeId);
	bool CampfireCardLoss(Guid cardId);
	UserData RetrieveUserData(string username);
	void CampfireUpgrade(Guid cardId);
	bool DoesUserExist(string username);
	bool DoesCardBelongToUser(Guid cardid, string username);
	bool DoesDealIdAlreadyExist(Guid tradeId);
	bool IsCardInUserDeck(Guid cardid, string username);
	Guid? RetrieveCardidFromTradeid(Guid tradeId);
	bool IsCardEngagedInTrade(Guid cardid);
	bool IsTokenValid(string authToken);
	string RetrieveUsernameFromToken(string authToken);
	List<ICard> RetrieveUserCards(string userName, string tableName);
	bool TableExists(string tableName);
	void CreateDbObject(string tableName, string createStatement);
	void InsertValues(string tableName, string insertStatement);
	List<TradingDeal> RetrieveTrades();
}
