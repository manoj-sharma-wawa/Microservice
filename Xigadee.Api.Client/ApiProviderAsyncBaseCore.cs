﻿#region using
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
#endregion
namespace Xigadee
{
    /// <summary>
    /// This is a more flexible Api client that supports more extensible authentication and Uri mapping support.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    public abstract class ApiProviderAsyncBaseCore<K, E>: IRepositoryAsync<K, E>
        where K : IEquatable<K>
    {
        #region Declarations
        /// <summary>
        /// This method is used to maps entity keys to a string represntation for the Uri.
        /// </summary>
        protected IKeyMapper<K> mKeyMapper;
        /// <summary>
        /// This class is used to translate the requests to the appropriate Uri.
        /// </summary>
        protected TransportUriMapper<K> mUriMapper;
        /// <summary>
        /// This is the collection of transports available for serialization.
        /// </summary>
        protected Dictionary<string, TransportSerializer<E>> mTransportSerializers;
        /// <summary>
        /// This is the primary transport used for sending requests.
        /// </summary>
        protected TransportSerializer<E> mPrimaryTransport;
        /// <summary>
        /// This is the assembly version
        /// </summary>
        protected readonly string mAssemblyVersion;
        /// <summary>
        /// This is a list of auth handlers to be used to au
        /// </summary>
        protected List<IApiProviderAuthBase> mAuthHandlers;
        #endregion
        #region Constructor
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public ApiProviderAsyncBaseCore(TransportUriMapper<K> uriMapper = null
            , bool useHttps = true
            , string entityName = null
            , IKeyMapper<K> keyMapper = null
            , TransportSerializer<E> primaryTransport = null
            , IEnumerable<IApiProviderAuthBase> authHandlers = null)
        {
            // Get the types assembly version to add to the request headers
            mAssemblyVersion = AssemblyVersionGet();

            mKeyMapper = keyMapper ?? ResolveKeyMapper();

            mUriMapper = uriMapper ?? ResolveUriMapper(uriMapper, entityName);

            mPrimaryTransport = primaryTransport ?? ResolveTransport();

            mAuthHandlers = authHandlers?.ToList();
        }
        #endregion

        #region AssemblyVersionGet()
        /// <summary>
        /// This method returns the assembly version that is passed to the calling party. You can override this
        /// method to change the version.
        /// </summary>
        /// <returns>Returns a string containing the assembly version.</returns>
        protected virtual string AssemblyVersionGet()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }  
        #endregion

        #region ResolveKeyMapper()
        /// <summary>
        /// This method resolves the mapper that converts the key in to a format suitable for the Uri request.
        /// </summary>
        protected virtual IKeyMapper<K> ResolveKeyMapper()
        {
            return (KeyMapper<K>)KeyMapper.Resolve<K>();
        }
        #endregion
        #region ResolveUriMapper(TransportUriMapper<K> mapper)
        /// <summary>
        /// This method resolves the appropriate transport uri mapper for the request.
        /// </summary>
        /// <param name="uriMapper">The mapper passed through the constructor.</param>
        protected virtual TransportUriMapper<K> ResolveUriMapper(TransportUriMapper<K> uriMapper, string entityName)
        {
            return new TransportUriMapper<K>(mKeyMapper);
        }
        #endregion
        #region ResolveTransport()
        /// <summary>
        /// This method resolves the specific serializer for the entity transport.
        /// </summary>
        protected virtual TransportSerializer<E> ResolveTransport()
        {
            mTransportSerializers = TransportSerializer.GetSerializers<E>(GetType()).ToDictionary((s) => s.MediaType.ToLowerInvariant());

            if (mTransportSerializers == null || mTransportSerializers.Count == 0)
                mTransportSerializers = TransportSerializer.GetSerializers<E>(typeof(E)).ToDictionary((s) => s.MediaType.ToLowerInvariant());

            if (mTransportSerializers == null || mTransportSerializers.Count == 0)
                throw new TransportSerializerResolutionException("The default TransportSerializer cannot be resolved.");

            //Get the transport serializer with the highest priority.
            return mTransportSerializers.Values.OrderByDescending((t) => t.Priority).First();
        }
        #endregion

        #region Create(E entity, RepositorySettings options = null)
        /// <summary>
        /// This is the Create method.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <param name="options">The optional parameters.</param>
        /// <returns>Returns the response with the entity if the request is successful.</returns>
        public virtual async Task<RepositoryHolder<K, E>> Create(E entity, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Post);

            using (var content = EntitySerialize(entity))
            {
                return await CallClient<K, E>(uri, options
                    , content: content
                    , deserializer: EntityDeserialize
                    , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
            }
        }
        #endregion
        #region Read(K key, RepositorySettings options = null)
        /// <summary>
        /// This method reads an entity based on the key passed.
        /// </summary>
        /// <param name="key">The key request.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, E>> Read(K key, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Get, key);

            return await CallClient<K, E>(uri, options
                , deserializer: EntityDeserialize
                , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
        }
        #endregion
        #region ReadByRef(string refKey, string refValue, RepositorySettings options = null)
        /// <summary>
        /// This method reads a reference based on its reference key and value.
        /// </summary>
        /// <param name="refKey">The key type.</param>
        /// <param name="refValue">The key value.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, E>> ReadByRef(string refKey, string refValue, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Get, refKey, refValue);

            return await CallClient<K, E>(uri, options
                , deserializer: EntityDeserialize
                , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
        }
        #endregion
        #region Update(E entity, RepositorySettings options = null)
        /// <summary>
        /// This method updates the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, E>> Update(E entity, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Put);

            using (var content = EntitySerialize(entity))
            {
                return await CallClient<K, E>(uri, options
                    , content: content
                    , deserializer: EntityDeserialize
                    , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
            }
        }
        #endregion
        #region Delete(K key, RepositorySettings options = null)
        /// <summary>
        /// This method deletes an entity.
        /// </summary>
        /// <param name="key">The entity unique key.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, Tuple<K, string>>> Delete(K key, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Delete, key);

            return await CallClient<K, Tuple<K, string>>(uri, options
                , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
        }
        #endregion
        #region DeleteByRef(string refKey, string refValue, RepositorySettings options = null)
        /// <summary>
        /// This method deletes the entity based on the reference key and value.
        /// </summary>
        /// <param name="refKey">The key type.</param>
        /// <param name="refValue">The key value.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, Tuple<K, string>>> DeleteByRef(string refKey, string refValue, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Delete, refKey, refValue);

            return await CallClient<K, Tuple<K, string>>(uri, options
                , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
        }
        #endregion
        #region Version(K key, RepositorySettings options = null)
        /// <summary>
        /// This method resolves the version id of the entity where supported.
        /// </summary>
        /// <param name="key">The entity unique key.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, Tuple<K, string>>> Version(K key, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Head, key);

            return await CallClient<K, Tuple<K, string>>(uri, options
                , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
        }
        #endregion
        #region VersionByRef(string refKey, string refValue, RepositorySettings options = null)
        /// <summary>
        /// Thi method resolves the entity id and version based on the reference parameters.
        /// </summary>
        /// <param name="refKey">The key type.</param>
        /// <param name="refValue">The key value.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<K, Tuple<K, string>>> VersionByRef(string refKey, string refValue, RepositorySettings options = null)
        {
            var uri = GetUri(HttpMethod.Head, refKey, refValue);

            return await CallClient<K, Tuple<K, string>>(uri, options
                , mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
        }
        #endregion

        #region Search(K key, RepositorySettings options = null)
        /// <summary>
        /// This method reads an entity based on the key passed.
        /// </summary>
        /// <param name="key">The key request.</param>
        /// <param name="options">These are the repository options which define the request type..</param>
        /// <returns>This is the holder containing the response and the entity where necessary.</returns>
        public virtual async Task<RepositoryHolder<SearchRequest, SearchResponse>> Search(SearchRequest key, RepositorySettings options = null)
        {
            var uri = mUriMapper.MakeUri(new HttpMethod("Search"));

            throw new NotImplementedException();
            //using (var content = GetContent(entity))
            //{
            //    return await CallClient<K, E>(uri, options, content: content, deserializer: DeserializeEntity,
            //        mapper: (rs, holder) => ExtractHeaders(rs, holder, mKeyMapper));
            //}
        }
        #endregion

        #region ExtractHeaders<KT, ET>(HttpResponseMessage rs, RepositoryHolder<KT, ET> holder, IKeyMapper<KT> keyMapper)
        /// <summary>
        /// This method resolves the appropriate transport serialzer from the incoming accept header.
        /// </summary>
        /// <param name="rs">The incoming response.</param>
        /// <param name="transport">The resolve transport serializer.</param>
        /// <returns>Returns true if the serializer can be resolved.</returns>
        protected virtual void ExtractHeaders<KT>(HttpResponseMessage rs, RepositoryHolder<KT> holder, IKeyMapper<KT> keyMapper)
        {
            string contentId = rs.Headers.Where((h) => h.Key.Equals("x-img-contentid", StringComparison.InvariantCultureIgnoreCase))
                .SelectMany((h) => h.Value).FirstOrDefault() ?? "";

            string versionId = rs.Headers.Where((h) => h.Key.Equals("x-img-versionid", StringComparison.InvariantCultureIgnoreCase))
                .SelectMany((h) => h.Value).FirstOrDefault() ?? "";

            holder.KeyReference = new Tuple<string, string>(contentId, versionId);

            if (!string.IsNullOrEmpty(contentId))
                holder.Key = keyMapper.ToKey(contentId);
        }
        #endregion

        #region CallClient<KT,ET>...
        /// <summary>
        /// This method calls the client using HTTP and returns the response along with the entity in the response if supplied.
        /// </summary>
        /// <typeparam name="KT">The key type.</typeparam>
        /// <typeparam name="ET">The entity type.</typeparam>
        /// <param name="uri">The request uri.</param>
        /// <param name="options">The repository settings passed from the caller.</param>
        /// <param name="content">The HttpContent to send to the API.</param>
        /// <param name="adjust">Any message adjustment.</param>
        /// <param name="mapper">Any response adjustment before returning to the caller.</param>
        /// <param name="deserializer">Deserialize the response content into the entity</param>
        /// <returns></returns>
        protected virtual async Task<RepositoryHolder<KT, ET>> CallClient<KT, ET>(
              KeyValuePair<HttpMethod, Uri> uri
            , RepositorySettings options
            , HttpContent content = null
            , Action<HttpRequestMessage> adjust = null
            , Action<HttpResponseMessage, RepositoryHolder<KT, ET>> mapper = null
            , Action<HttpResponseMessage, byte[], RepositoryHolder<KT, ET>> deserializer = null)
        {
            var response = new RepositoryHolder<KT, ET>();

            try
            {
                HttpRequestMessage httpRq = Request(uri.Key, uri.Value);

                RequestHeadersSet(httpRq);

                RequestHeadersPreferSet(httpRq, options?.Prefer);

                RequestHeadersAuthSet(httpRq);

                adjust?.Invoke(httpRq);

                if (content != null)
                    httpRq.Content = content;

                var client = new HttpClient();

                // Specify request body
                var httpRs = await client.SendAsync(httpRq);

                //OK, set the response content if set
                if (httpRs.Content != null && httpRs.Content.Headers.ContentLength > 0)
                {
                    byte[] httpRsContent = await httpRs.Content.ReadAsByteArrayAsync();

                    if (httpRs.IsSuccessStatusCode)
                        deserializer?.Invoke(httpRs, httpRsContent, response);
                    else
                        // So that we can see error messages such as schema validation fail
                        response.ResponseMessage = Encoding.UTF8.GetString(httpRsContent);
                }

                //Get any outgoing trace headers and set them in to the response.
                IEnumerable<string> trace;
                if (httpRs.Headers.TryGetValues(ApimConstants.AzureTraceHeaderLocation, out trace))
                    response.Settings.Prefer.Add(ApimConstants.AzureTraceHeaderLocation, trace.First());

                response.ResponseCode = (int)httpRs.StatusCode;

                mapper?.Invoke(httpRs, response);
            }
            catch (Exception ex)
            {
                response.ResponseMessage = FormatExceptionChain(ex);
                response.ResponseCode = 503;
            }

            return response;
        }
        #endregion

        #region Request(HttpMethod verb, Uri uri)
        /// <summary>
        /// This method creates the default request message.
        /// </summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="uri">The Uri request.</param>
        /// <returns>Returns the message with the full domain request.</returns>
        protected virtual HttpRequestMessage Request(HttpMethod verb, Uri uri)
        {
            HttpRequestMessage rq = new HttpRequestMessage
            {
                Method = verb,
                RequestUri = uri
            };

            return rq;
        }
        #endregion

        #region RequestHeadersSet(HttpRequestMessage rq)
        /// <summary>
        /// This virtual method sets the necessary headers for the request.
        /// </summary>
        /// <param name="rq">The http request.</param>
        protected virtual void RequestHeadersSet(HttpRequestMessage rq)
        {
            RequestHeadersSetHelper(rq);

            RequestHeadersSetTransport(rq);

            RequestHeadersSetSecurity(rq);
        }
        #endregion
        #region RequestHeadersSetHelper(HttpRequestMessage rq)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rq"></param>
        protected virtual void RequestHeadersSetHelper(HttpRequestMessage rq)
        {
            rq.Headers.Add("x-api-clientversion", mAssemblyVersion);
            rq.Headers.Add("x-api-version", "2016-08-01");
        }
        #endregion
        #region RequestHeadersSetTransport(HttpRequestMessage rq)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rq"></param>
        protected virtual void RequestHeadersSetTransport(HttpRequestMessage rq)
        {
            mTransportSerializers.ForEach((t) => rq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(t.Value.MediaType, t.Value.Priority)));
        }
        #endregion
        #region RequestHeadersSetSecurity(HttpRequestMessage rq)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rq">The http request object.</param>
        protected virtual void RequestHeadersSetSecurity(HttpRequestMessage rq)
        {

        }
        #endregion

        #region RequestHeadersPreferSet(HttpRequestMessage rq, Dictionary<string, string> Prefer)
        /// <summary>
        /// This method sets the prefer request headers for the Api call.
        /// </summary>
        /// <param name="rq">The http request object.</param>
        /// <param name="Prefer">The prefer collection.</param>
        protected virtual void RequestHeadersPreferSet(HttpRequestMessage rq, Dictionary<string, string> Prefer)
        {
            if (Prefer != null && Prefer.Count > 0)
                rq.Headers.Add("Prefer", Prefer.Select((k) => string.Format("{0}={1}", k.Key, k.Value)));
        }
        #endregion

        #region RequestHeadersAuthSet(HttpRequestMessage rq)
        /// <summary>
        /// This method sets the prefer request headers for the Api call.
        /// </summary>
        /// <param name="rq">The http request object.</param>
        /// <param name="Prefer">The prefer collection.</param>
        protected virtual void RequestHeadersAuthSet(HttpRequestMessage rq)
        {
        }
        #endregion

        #region EntitySerialize(E entity)
        /// <summary>
        /// This method turns the entity in to binary content using
        /// the primary transport
        /// </summary>
        /// <param name="entity">The entity to convert.</param>
        /// <returns>The ByteArrayContent to transmit.</returns>
        protected virtual ByteArrayContent EntitySerialize(E entity)
        {
            var data = mPrimaryTransport.GetData(entity);
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mPrimaryTransport.MediaType);
            return content;
        }
        #endregion
        #region EntityDeserialize(ApiRequest rq, out TransportSerializer<E> transport)
        /// <summary>
        /// This method resolves the appropriate transport serialzer from the incoming accept header.
        /// </summary>
        /// <param name="rs">The response</param>
        /// <param name="data">The response content</param>
        /// <param name="holder">The repository holder</param>
        /// <returns>Returns true if the serializer can be resolved.</returns>
        protected virtual void EntityDeserialize(HttpResponseMessage rs, byte[] data, RepositoryHolder<K, E> holder)
        {
            string mediaType = rs.Content.Headers.ContentType.MediaType;
            if (mTransportSerializers.ContainsKey(mediaType))
            {
                var transport = mTransportSerializers[mediaType];
                holder.Entity = transport.GetObject(data);
            }
            else
                throw new TransportSerializerResolutionException(mediaType);
        }
        #endregion

        #region FormatExceptionChain(Exception exception, string message = null)
        /// <summary>
        /// Formats the exception message including all inner exceptions. Useful when we have a send error on the API
        /// which might be caused by a DNS or a certificate issue etc. 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string FormatExceptionChain(Exception exception, string message = null)
        {
            if (exception == null)
                return message;

            message = string.IsNullOrEmpty(message) ? exception.Message : string.Format("{0} {1}", message, exception.Message);

            return FormatExceptionChain(exception.InnerException, message);
        }
        #endregion

        protected virtual KeyValuePair<HttpMethod, Uri> GetUri(HttpMethod method)
        {
            return mUriMapper.MakeUri(method);
        }

        protected virtual KeyValuePair<HttpMethod, Uri> GetUri(HttpMethod method, K key)
        {
            return mUriMapper.MakeUri(method, key);
        }

        protected virtual KeyValuePair<HttpMethod, Uri> GetUri(HttpMethod method, string refKey, string refValue)
        {
            return mUriMapper.MakeUri(method, refKey, refValue);
        }
    }
}