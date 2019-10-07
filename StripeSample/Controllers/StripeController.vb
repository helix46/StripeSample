'Imports System.Web.Mvc

'Namespace Controllers
'    Public Class StripeController
'        Inherits Controller

'        ' GET: Stripe
'        Function Index() As ActionResult
'            Return View()
'        End Function
'    End Class
'End Namespace

Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Web.Http
Imports System.Security.Principal

Public Structure StructStripe
    Public chargeToken As String
    Public amount As Decimal
End Structure

Namespace Controllers
    <RoutePrefix("api/stripe")>
    Public Class StripeController
        Inherits ApiController

        <HttpPost> <Route> '<Authorize>
        Public Async Function [post](StructStripe As StructStripe) As Threading.Tasks.Task(Of IHttpActionResult)
            Try
                Dim ProductionMode = False
                Dim StripeApiSecretKeyLive = ""
                Dim StripeApiSecretKeyDev = "sk_test_xxxxxxxxxxxxxxxxxxxxxxxx"
                Dim StructStripePaymentReturn As StructStripePaymentReturn = Await StripeService.makeStripeCharge(StructStripe.amount, StructStripe.chargeToken, ProductionMode, StripeApiSecretKeyLive, StripeApiSecretKeyDev)
                If StructStripePaymentReturn.Paid Then
                    'save to database, send receipt etc
                    Return Ok("Charge successful")
                Else
                    Return Ok(StructStripePaymentReturn.ErrorMessage)
                End If

            Catch ex As Exception
                ' handle error
                Return InternalServerError(ex)
            End Try
        End Function

    End Class
End Namespace