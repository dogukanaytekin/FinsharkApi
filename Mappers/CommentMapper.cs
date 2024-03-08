using api.Dtos.Comment;
using api.Models;

namespace api.Mappers
{
    public static class CommentMapper
    {
        public static CommentDto ToCommentDto(this Comment commentModel)
        {
            return new CommentDto 
            {
                Id=commentModel.Id,
                Title = commentModel.Title,
                Content = commentModel.Content,
                CreatedOn = commentModel.CreatedOn,
                StockId = commentModel.StockId
            };
        }

        public static Comment ToCreateCommentDtoFromComment (this CreateCommentDto createDto , int stockId)
        {
            return new Comment 
            {
                Title = createDto.Title,
                Content = createDto.Content,
                StockId = stockId
            };
        }
    }
}