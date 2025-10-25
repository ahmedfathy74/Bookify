using Bookify.Application.Common.Models;
using Bookify.Domain.Consts;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Persistence.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<T> GetAll(bool withNoTracking = true)
        {
            IQueryable<T> query = _context.Set<T>();

            if (withNoTracking)
                query = query.AsNoTracking();

            return query.ToList();
        }

        public IQueryable<T> GetQueryable() => _context.Set<T>();

        public T? GetById(int id) => _context.Set<T>().Find(id);

        public PaginatedList<T> GetPaginatedList(IQueryable<T> query, int pageNumber, int pageSize)
        {
            return PaginatedList<T>.Create(query, pageNumber, pageSize);
        }

        public T? Find(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().SingleOrDefault(predicate);
        }

        public T? Find(Expression<Func<T, bool>> predicate, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if(includes is not null)
                foreach (var include in includes)
                    query = query.Include(include);

            return query.SingleOrDefault(predicate);
        }

        public T? Find(Expression<Func<T, bool>> predicate, Func<IQueryable<T>,
            IIncludableQueryable<T, object>>? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();

            if (includes is not null)
                query = includes(query);

            return query.SingleOrDefault(predicate);
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, int? skip = null, int? take = null,
            Expression<Func<T, object>>? orderBy = null, string? orderByDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);

            if (orderBy is not null)
            {
                query = orderByDirection == OrderBy.Ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return query.ToList();
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>? orderBy = null, 
            string? orderByDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);

            if (orderBy is not null)
            {
                query = orderByDirection == OrderBy.Ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            return query.ToList();
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, 
            IIncludableQueryable<T, object>>? includes = null, Expression<Func<T, object>>? orderBy = null,
            string? orderByDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();

            if (includes is not null)
                query = includes(query);

            query = query.Where(predicate);

            if (orderBy is not null)
            {
                query = orderByDirection == OrderBy.Ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            return query.ToList();
        }

        public T Add(T entity)
        {
            _context.Add(entity);
            return entity;
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
            return entities;
        }

        public void Update(T entity) => _context.Update(entity);

        public void Remove(T entity) => _context.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _context.RemoveRange(entities);

        public void DeleteBulk(Expression<Func<T, bool>> predicate) => 
            _context.Set<T>().Where(predicate).ExecuteDelete();

        public bool IsExists(Expression<Func<T, bool>> predicate) =>
            _context.Set<T>().Any(predicate);

        public int Count() => _context.Set<T>().Count();

        public int Count(Expression<Func<T, bool>> predicate) => _context.Set<T>().Count(predicate);

        public int Max(Expression<Func<T, bool>> predicate, Expression<Func<T, int>> field) =>
            _context.Set<T>().Any(predicate) ? _context.Set<T>().Where(predicate).Max(field) : 0;

    }
}
