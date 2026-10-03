package com.corhuila.booksapi.config;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Contact;
import io.swagger.v3.oas.models.info.Info;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class OpenApiConfig {

    @Bean
    public OpenAPI booksOpenAPI() {
        return new OpenAPI().info(new Info()
                .title("Books API")
                .version("1.0.0")
                .description("REST API with layered architecture (controller, service, repository, entity) "
                        + "and JPA persistence. CORHUILA - Estructura de Datos 2026-B.")
                .contact(new Contact().name("Jesús Ariel González Bonilla").url("https://github.com/ariel5253")));
    }
}
