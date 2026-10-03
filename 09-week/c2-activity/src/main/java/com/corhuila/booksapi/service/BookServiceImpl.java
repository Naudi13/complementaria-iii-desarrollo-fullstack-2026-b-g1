package com.corhuila.booksapi.service;

import com.corhuila.booksapi.dto.BookRequest;
import com.corhuila.booksapi.dto.BookResponse;
import com.corhuila.booksapi.entity.Book;
import com.corhuila.booksapi.exception.DuplicateResourceException;
import com.corhuila.booksapi.exception.ResourceNotFoundException;
import com.corhuila.booksapi.repository.BookRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@Transactional(readOnly = true)
public class BookServiceImpl implements BookService {

    private static final String RESOURCE = "Book";

    private final BookRepository repository;

    public BookServiceImpl(BookRepository repository) {
        this.repository = repository;
    }

    @Override
    public List<BookResponse> findAll() {
        return repository.findAll().stream().map(BookResponse::from).toList();
    }

    @Override
    public BookResponse findById(Long id) {
        return BookResponse.from(getOrThrow(id));
    }

    @Override
    @Transactional
    public BookResponse create(BookRequest request) {
        if (repository.existsByIsbn(request.isbn())) {
            throw new DuplicateResourceException("A book with ISBN " + request.isbn() + " already exists");
        }
        Book book = new Book();
        apply(book, request);
        return BookResponse.from(repository.save(book));
    }

    @Override
    @Transactional
    public BookResponse update(Long id, BookRequest request) {
        Book book = getOrThrow(id);
        if (repository.existsByIsbnAndIdNot(request.isbn(), id)) {
            throw new DuplicateResourceException("A book with ISBN " + request.isbn() + " already exists");
        }
        apply(book, request);
        return BookResponse.from(repository.save(book));
    }

    @Override
    @Transactional
    public void delete(Long id) {
        repository.delete(getOrThrow(id));
    }

    private Book getOrThrow(Long id) {
        return repository.findById(id).orElseThrow(() -> new ResourceNotFoundException(RESOURCE, id));
    }

    private void apply(Book book, BookRequest request) {
        book.setTitle(request.title());
        book.setAuthor(request.author());
        book.setIsbn(request.isbn());
        book.setPublishedYear(request.publishedYear());
        book.setPrice(request.price());
    }
}
