import { TestBed } from '@angular/core/testing';

import { UserListResolverService } from './users-resolvers.service';

describe('UserListResolverService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UserListResolverService = TestBed.get(UserListResolverService);
    expect(service).toBeTruthy();
  });
});
