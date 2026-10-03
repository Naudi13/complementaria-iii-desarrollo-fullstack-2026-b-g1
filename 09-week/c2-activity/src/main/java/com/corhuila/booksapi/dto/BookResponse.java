package com.corhuila.booksapi.dto;

import com.corhuila.booksapi.entity.Book;

import java.math.BigDecimal;

public record BookResponse(
        Long id,
        String title,
        String author,
        String isbn,
        Integer publishedYear,
        BigDecimal price
) {
    public static BookResponse from(Book book) {
        return new BookResponse(
                book.getId(),
                book.getTitle(),
                book.getAuthor(),
                book.getIsbn(),
                book.getPublishedYear(),
                book.getPrice());
    }
}
