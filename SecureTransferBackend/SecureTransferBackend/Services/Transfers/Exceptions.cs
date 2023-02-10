namespace SecureTransferBackend.Services.Transfers;

public class TransfersException : Exception
{
}

public class WrongKeyForUserException : TransfersException
{
}

public class NotOwnerOfBundleException : TransfersException
{
}

public class BundleNotFoundException : TransfersException
{
}