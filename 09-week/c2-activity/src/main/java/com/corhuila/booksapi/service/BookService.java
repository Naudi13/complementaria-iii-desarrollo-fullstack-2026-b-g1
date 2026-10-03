package com.corhuila.booksapi.service;

import com.corhuila.booksapi.dto.BookRequest;
import com.corhuila.booksapi.dto.BookResponse;

import java.util.List;

public interface BookService {

    List<BookResponse> findAll();

    BookResponse findById(Long id);

    BookResponse create(BookRequest request);

    BookResponse update(Long id, BookRequest request);

    void delete(Long id);
}
