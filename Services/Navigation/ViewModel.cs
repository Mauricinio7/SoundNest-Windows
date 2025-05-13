using Services.Communication.RESTful.Models;
using Services.Infrestructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Navigation
{
    public abstract class ViewModel : ObservableObject
    {
        protected async Task<ApiResult<T>> ExecuteRESTfulApiCall<T>(Func<Task<ApiResult<T>>> action)
        {
            try
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                return await action();
            }
            catch (HttpRequestException)
            {
                return ApiResult<T>.Failure("No se pudo conectar con el servidor.", "Error de red");
            }
            catch (TaskCanceledException)
            {
                return ApiResult<T>.Failure("La operación se canceló por timeout.", "Timeout");
            }
            catch (JsonException)
            {
                return ApiResult<T>.Failure("La respuesta no se pudo interpretar.", "Error de deserialización");
            }
            catch (Exception)
            {
                return ApiResult<T>.Failure("Ocurrió un error inesperado", "Error inesperado");
            }
            finally
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
            }
        }


    }
}
