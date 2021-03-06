﻿#region using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
#endregion
namespace Xigadee
{
    #region IRepositoryAsyncServer<K, E>
    /// <summary>
    /// This is a default repository async interface for entities within the system.
    /// </summary>
    /// <typeparam name="K">The entity key object.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    public interface IRepositoryAsyncServer<K, E> : IRepositoryAsync<K, E>
        where K : IEquatable<K>
    {
        /// <summary>
        /// Gets or sets the default security principal.
        /// </summary>
        IPrincipal DefaultPrincipal { get; set; }
    }
    #endregion

    #region IRepositoryAsync<K, E>
    /// <summary>
    /// This is a default repository async interface for entities within the system.
    /// </summary>
    /// <typeparam name="K">The entity key object.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    public interface IRepositoryAsync<K, E>
        where K : IEquatable<K>
    {
        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, E>> Create(E entity, RepositorySettings options = null);
        /// <summary>
        /// Reads the entity by the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, E>> Read(K key, RepositorySettings options = null);
        /// <summary>
        /// Reads the entity by a reference key-value pair.
        /// </summary>
        /// <param name="refKey">The reference key.</param>
        /// <param name="refValue">The reference value.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, E>> ReadByRef(string refKey, string refValue, RepositorySettings options = null);
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, E>> Update(E entity, RepositorySettings options = null);
        /// <summary>
        /// Deletes the entity by the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, Tuple<K, string>>> Delete(K key, RepositorySettings options = null);
        /// <summary>
        /// Deletes the entity by reference.
        /// </summary>
        /// <param name="refKey">The reference key.</param>
        /// <param name="refValue">The reference value.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, Tuple<K, string>>> DeleteByRef(string refKey, string refValue, RepositorySettings options = null);
        /// <summary>
        /// Validates the version by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, Tuple<K, string>>> Version(K key, RepositorySettings options = null);
        /// <summary>
        /// Validates the version by reference.
        /// </summary>
        /// <param name="refKey">The reference key.</param>
        /// <param name="refValue">The reference value.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<K, Tuple<K, string>>> VersionByRef(string refKey, string refValue, RepositorySettings options = null);
        /// <summary>
        /// Searches the entity store.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="options">The repository options settings.</param>
        /// <returns>Returns the holder with the response and data.</returns>
        Task<RepositoryHolder<SearchRequest, SearchResponse>> Search(SearchRequest key, RepositorySettings options = null);
    } 
    #endregion
}