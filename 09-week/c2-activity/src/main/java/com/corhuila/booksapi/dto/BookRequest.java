package com.corhuila.booksapi.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;

import java.math.BigDecimal;

@Schema(description = "Payload used to create or update a book")
public record BookRequest(

        @Schema(example = "Clean Code")
        @NotBlank(message = "title is required")
        @Size(max = 150, message = "title must have at most 150 characters")
        String title,

        @Schema(example = "Robert C. Martin")
        @NotBlank(message = "author is required")
        @Size(max = 100, message = "author must have at most 100 characters")
        String author,

        @Schema(example = "9780132350884")
        @NotBlank(message = "isbn is required")
        @Size(min = 10, max = 17, message = "isbn must have between 10 and 17 characters")
        String isbn,

        @Schema(example = "2008")
        @NotNull(message = "publishedYear is required")
        @Min(value = 1450, message = "publishedYear must be >= 1450")
        @Max(value = 2100, message = "publishedYear must be <= 2100")
        Integer publishedYear,

        @Schema(example = "45.90")
        @NotNull(message = "price is required")
        @DecimalMin(value = "0.0", message = "price must be >= 0")
        BigDecimal price
) {
}
