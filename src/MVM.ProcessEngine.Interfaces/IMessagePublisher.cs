using System;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Interfaces
{
    /// <summary>
    /// Define el contrato para publicar mensajes en una cola.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publica un mensaje en una implementación de un Message Bus.
        /// </summary>
        /// <param name="message">Mensaje que se publicará.</param>
        /// <returns>Identificador de la conversación</returns>
        void Publish(string tenant, object message);

    
        /// <summary>
        /// Obtiene o establece el identíficador de la conversación.
        /// </summary>
        Guid ConversationHandleId { get; }

        Task PublishAsync(string tenant, object message);
    }
}