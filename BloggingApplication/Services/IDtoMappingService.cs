namespace BloggingApplication.Services
{
    public interface IDtoMappingService<TDto, TEntity>
    {
        public Task<TDto> EntityToDto(TEntity entity);
        public Task<TEntity> DtoToEntity(TDto dto);
    }
}
