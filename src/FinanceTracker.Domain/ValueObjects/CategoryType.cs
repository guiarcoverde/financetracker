namespace FinanceTracker.Domain.ValueObjects;

public enum CategoryType
{
    Food = 1,
    Health = 2,
    Entertainment = 3,
    Transport = 4,
    Education = 5,
    Housing = 6,
    Clothing = 7,
    Technology = 8,
    Travel = 9,
    Utilities = 10,
    Insurance = 11,
    PersonalCare = 12,
    Gifts = 13,
    Other = 14,
    
    Salary = 100,
    Freelance = 101,
    Investment = 102,
    Bonus = 103,
    Gift = 104,
    Refund = 105,
    SideIncome = 106
}

public static class CategoryTypeExtensions
{
    public static string GetDisplayName(this CategoryType categoryType)
    {
        return categoryType switch
        {
            CategoryType.Food => "Alimentação",
            CategoryType.Health => "Saúde",
            CategoryType.Entertainment => "Lazer",
            CategoryType.Transport => "Transporte",
            CategoryType.Education => "Educação",
            CategoryType.Housing => "Moradia",
            CategoryType.Clothing => "Vestuário",
            CategoryType.Technology => "Tecnologia",
            CategoryType.Utilities => "Serviços Públicos",
            CategoryType.Insurance => "Seguros",
            CategoryType.Travel => "Viagem",
            CategoryType.PersonalCare => "Cuidados Pessoais",
            CategoryType.Gifts => "Presentes",
            CategoryType.Other  => "Outros",
            
            CategoryType.Salary => "Salário",
            CategoryType.Freelance => "Freelance",
            CategoryType.Investment => "Investimentos",
            CategoryType.Gift => "Presente Recebido",
            CategoryType.Bonus => "Bônus",
            CategoryType.Refund => "Reembolso",
            CategoryType.SideIncome => "Renda Extra",
            
            _ => categoryType.ToString()
        };
    }

    public static TransactionType GetTransactionType(this CategoryType categoryType)
    {
        return categoryType switch
        {
            >= CategoryType.Food and <= CategoryType.Other => TransactionType.Expense,
            >= CategoryType.Salary and <= CategoryType.SideIncome => TransactionType.Income,
            _ => throw new ArgumentException($"Categoria inválida: {categoryType}")
        };
    }

    public static bool IsExpenseCategory(this CategoryType categoryType)
    {
        return categoryType >= CategoryType.Food && categoryType <= CategoryType.Other;
    }

    public static bool IsIncomeCategory(this CategoryType categoryType)
    {
        return categoryType >= CategoryType.Salary && categoryType <= CategoryType.SideIncome;
    }
}