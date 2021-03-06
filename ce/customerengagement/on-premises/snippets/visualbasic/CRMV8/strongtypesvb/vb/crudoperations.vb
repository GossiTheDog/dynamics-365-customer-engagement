' <snippetcrudoperations>


Imports System.ServiceModel
Imports System.ServiceModel.Description

' These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
' located in the SDK\bin folder of the SDK download.
Imports Microsoft.Xrm.Sdk
Imports Microsoft.Xrm.Sdk.Query
Imports Microsoft.Xrm.Sdk.Client

Namespace Microsoft.Crm.Sdk.Samples
    ''' <summary>
    ''' Demonstrates how to do basic entity operations like create, retrieve,
    ''' update, and delete.</summary>
    ''' <remarks>
    ''' At run-time, you will be given the option to delete all the
    ''' database records created by this program.</remarks>
    Public Class CRUDOperations
        #Region "Class Level Members"

        Private _accountId As Guid
        Private _parentAccountId As Guid
        Private _serviceProxy As OrganizationServiceProxy

        #End Region ' Class Level Members

        #Region "How To Sample Code"
        ''' <summary>
        ''' This method first connects to the Organization service. Afterwards,
        ''' basic create, retrieve, update, and delete entity operations are performed.
        ''' </summary>
        ''' <param name="serverConfig">Contains server connection information.</param>
        ''' <param name="promptforDelete">When True, the user will be prompted to delete all
        ''' created entities.</param>
        Public Sub Run(ByVal serverConfig As ServerConnection.Configuration, ByVal promptforDelete As Boolean)
            Try
                ' Connect to the Organization service. 
                ' The using statement assures that the service proxy will be properly disposed.
                _serviceProxy = ServerConnection.GetOrganizationProxy(serverConfig)
                Using _serviceProxy
                    ' This statement is required to enable early-bound type support.
                    _serviceProxy.EnableProxyTypes()


                    CreateRequiredRecords()

                    ' Instantiate an account object.
                    ' See the Entity Metadata topic in the SDK documentation to determine 
                    ' which attributes must be set for each entity.
                    Dim account_Renamed As Account = New Account With {.Name = "Fourth Coffee"}

                    ' Create an account record named Fourth Coffee.
                    _accountId = _serviceProxy.Create(account_Renamed)
                    Console.Write("{0} {1} created, ", account_Renamed.LogicalName, account_Renamed.Name)

                    ' Retrieve the account containing several of its attributes.
                    Dim cols As New ColumnSet(New String() {"name", "address1_postalcode", "lastusedincampaign", "versionnumber"})

                    Dim retrievedAccount As Account = CType(_serviceProxy.Retrieve("account", _accountId, cols), Account)
                    Console.Write("retrieved ")

                    ' Retrieve version number of the account. Shows BigInt attribute usage.
                    Dim versionNumber? As Long = retrievedAccount.VersionNumber

                    If versionNumber IsNot Nothing Then
                        Console.WriteLine("version # {0}, ", versionNumber)
                    End If

                    ' Update the postal code attribute.
                    retrievedAccount.Address1_PostalCode = "98052"

                    ' The address 2 postal code was set accidentally, so set it to null.
                    retrievedAccount.Address2_PostalCode = Nothing

                    ' Shows usage of option set (picklist) enumerations defined in OptionSets.cs.
                    retrievedAccount.Address1_AddressTypeCode = New OptionSetValue(CInt(Fix(AccountAddress1_AddressTypeCode.Primary)))
                    retrievedAccount.Address1_ShippingMethodCode = New OptionSetValue(CInt(Fix(AccountAddress1_ShippingMethodCode.DHL)))
                    retrievedAccount.IndustryCode = New OptionSetValue(CInt(Fix(AccountIndustryCode.AgricultureandNonpetrolNaturalResourceExtraction)))

                    ' Shows use of a Money value.
                    retrievedAccount.Revenue = New Money(5000000)

                    ' Shows use of a Boolean value.
                    retrievedAccount.CreditOnHold = False

                    ' Shows use of EntityReference.
                    retrievedAccount.ParentAccountId = New EntityReference(Account.EntityLogicalName, _parentAccountId)

                    ' Shows use of Memo attribute.
                    retrievedAccount.Description = "Account for Fourth Coffee."

                    ' Update the account record.
                    _serviceProxy.Update(retrievedAccount)
                    Console.WriteLine("and updated.")

                    DeleteRequiredRecords(promptforDelete)
                End Using

            ' Catch any service fault exceptions that Microsoft Dynamics CRM throws.
            Catch fe As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault)
                ' You can handle an exception here or pass it back to the calling method.
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Creates any entity records that this sample requires.
        ''' </summary>
        Public Sub CreateRequiredRecords()
            ' For this sample, all required entities are created in the Run() method.
            Dim account_Renamed As Account = New Account With {.Name = "Sample Parent Account"}
            _parentAccountId = _serviceProxy.Create(account_Renamed)
        End Sub

        ''' <summary>
        ''' Deletes any entity records that were created for this sample.
        ''' <param name="prompt">Indicates whether to prompt the user 
        ''' to delete the records created in this sample.</param>
        ''' </summary>
        Public Sub DeleteRequiredRecords(ByVal prompt As Boolean)
            Dim deleteRecords As Boolean = True

            If prompt Then
                Console.WriteLine(vbLf &amp; "Do you want these entity records deleted? (y/n) [y]: ")
                Dim answer As String = Console.ReadLine()

                deleteRecords = (answer.StartsWith("y") OrElse answer.StartsWith("Y") OrElse answer = String.Empty)
            End If

            If deleteRecords Then
                _serviceProxy.Delete(Account.EntityLogicalName, _accountId)
                _serviceProxy.Delete(Account.EntityLogicalName, _parentAccountId)
                Console.WriteLine("Entity records have been deleted.")
            End If
        End Sub

        #End Region ' How To Sample Code

        #Region "Main method"

        ''' <summary>
        ''' Standard Main() method used by most SDK samples.
        ''' </summary>
        ''' <param name="args"></param>
        Public Shared Sub Main(ByVal args() As String)
            Try
                ' Obtain the target organization's Web address and client logon 
                ' credentials from the user.
                Dim serverConnect As New ServerConnection()
                Dim config As ServerConnection.Configuration = serverConnect.GetServerConfiguration()

                Dim app As New CRUDOperations()
                app.Run(config, True)

            Catch ex As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault)
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp)
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode)
                Console.WriteLine("Message: {0}", ex.Detail.Message)
                Console.WriteLine("Inner Fault: {0}", If(Nothing Is ex.Detail.InnerFault, "No Inner Fault", "Has Inner Fault"))
            Catch ex As TimeoutException
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine("Message: {0}", ex.Message)
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace)
                Console.WriteLine("Inner Fault: {0}", If(Nothing Is ex.InnerException.Message, "No Inner Fault", ex.InnerException.Message))
            Catch ex As Exception
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine(ex.Message)

                ' Display the details of the inner exception.
                If ex.InnerException IsNot Nothing Then
                    Console.WriteLine(ex.InnerException.Message)

                    Dim fe As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault) = TryCast(ex.InnerException,  _
                        FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault))
                    If fe IsNot Nothing Then
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp)
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode)
                        Console.WriteLine("Message: {0}", fe.Detail.Message)
                        Console.WriteLine("Trace: {0}", fe.Detail.TraceText)
                        Console.WriteLine("Inner Fault: {0}", If(Nothing Is fe.Detail.InnerFault, "No Inner Fault", "Has Inner Fault"))
                    End If
                End If
            ' Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            ' SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            Finally
                Console.WriteLine("Press <Enter> to exit.")
                Console.ReadLine()
            End Try
        End Sub
        #End Region ' Main method
    End Class
End Namespace

' </snippetcrudoperations>