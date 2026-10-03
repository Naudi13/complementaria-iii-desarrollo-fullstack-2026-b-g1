package com.corhuila.booksapi.controller;

import com.corhuila.booksapi.dto.BookRequest;
import com.corhuila.booksapi.dto.BookResponse;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.math.BigDecimal;
import java.util.UUID;

import static org.hamcrest.Matchers.hasSize;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.delete;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.put;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.header;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class BookControllerTest {

    private static final String URL = "/api/v1/books";

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper mapper;

    private BookRequest sample() {
        String isbn = UUID.randomUUID().toString().replace("-", "").substring(0, 13);
        return new BookRequest("Clean Code", "Robert C. Martin", isbn, 2008, new BigDecimal("45.90"));
    }

    private BookResponse createBook(BookRequest request) throws Exception {
        String json = mockMvc.perform(post(URL)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(mapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andReturn().getResponse().getContentAsString();
        return mapper.readValue(json, BookResponse.class);
    }

    @Test
    void createReturns201WithLocation() throws Exception {
        mockMvc.perform(post(URL)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(mapper.writeValueAsString(sample())))
                .andExpect(status().isCreated())
                .andExpect(header().exists("Location"))
                .andExpect(jsonPath("$.id").isNumber())
                .andExpect(jsonPath("$.title").value("Clean Code"));
    }

    @Test
    void createWithInvalidBodyReturns400() throws Exception {
        BookRequest invalid = new BookRequest("", "", "123", 1200, new BigDecimal("-1"));
        mockMvc.perform(post(URL)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(mapper.writeValueAsString(invalid)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.message").value("Validation failed"))
                .andExpect(jsonPath("$.details", hasSize(5)));
    }

    @Test
    void createWithDuplicateIsbnReturns409() throws Exception {
        BookRequest request = sample();
        createBook(request);
        mockMvc.perform(post(URL)
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(mapper.writeValueAsString(request)))
                .andExpect(status().isConflict());
    }

    @Test
    void getAllReturnsList() throws Exception {
        createBook(sample());
        mockMvc.perform(get(URL))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$").isArray());
    }

    @Test
    void getByIdReturnsBook() throws Exception {
        BookResponse created = createBook(sample());
        mockMvc.perform(get(URL + "/" + created.id()))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.isbn").value(created.isbn()));
    }

    @Test
    void getByIdNotFoundReturns404() throws Exception {
        mockMvc.perform(get(URL + "/999999"))
                .andExpect(status().isNotFound())
                .andExpect(jsonPath("$.status").value(404));
    }

    @Test
    void getByIdWithInvalidTypeReturns400() throws Exception {
        mockMvc.perform(get(URL + "/abc"))
                .andExpect(status().isBadRequest());
    }

    @Test
    void updateReturnsUpdatedBook() throws Exception {
        BookResponse created = createBook(sample());
        BookRequest changes = new BookRequest("Clean Code 2nd", "Robert C. Martin",
                created.isbn(), 2020, new BigDecimal("59.00"));
        mockMvc.perform(put(URL + "/" + created.id())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(mapper.writeValueAsString(changes)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.title").value("Clean Code 2nd"))
                .andExpect(jsonPath("$.publishedYear").value(2020));
    }

    @Test
    void updateNotFoundReturns404() throws Exception {
        mockMvc.perform(put(URL + "/999999")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(mapper.writeValueAsString(sample())))
                .andExpect(status().isNotFound());
    }

    @Test
    void deleteReturns204ThenGetReturns404() throws Exception {
        BookResponse created = createBook(sample());
        mockMvc.perform(delete(URL + "/" + created.id()))
                .andExpect(status().isNoContent());
        mockMvc.perform(get(URL + "/" + created.id()))
                .andExpect(status().isNotFound());
    }

    @Test
    void deleteNotFoundReturns404() throws Exception {
        mockMvc.perform(delete(URL + "/999999"))
                .andExpect(status().isNotFound());
    }
}
