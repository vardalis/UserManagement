import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'pager',
  templateUrl: './pager.component.html',
  styleUrls: ['./pager.component.css']
})
export class PagerComponent implements OnInit {
  @Input() items: Array<any>;
  @Input() totalItems: number;
  @Output() changePage = new EventEmitter<any>(true);
  @Input() initialPage = 1;
  @Input() pageSize = 10;

  private pager: any = {};

  constructor() { }

  ngOnInit() {
    // set page if items array isn't empty
    //if (this.items && this.items.length) {
    //  this.setPage(this.initialPage);
    //}

    if (this.totalItems) {
      this.pager = this.paginate(this.totalItems, this.initialPage, this.pageSize);
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    // reset page if items array has changed
    //if (changes.items.currentValue !== changes.items.previousValue) {
    //  this.setPage(this.initialPage);
    //}

    if ( (changes.totalItems && changes.totalItems.currentValue !== changes.totalItems.previousValue) ||
        (changes.initialPage && changes.initialPage.currentValue != changes.initialPage.previousValue) ) {
      this.pager = this.paginate(this.totalItems, this.initialPage, this.pageSize);
    }

    // this.pager = this.paginate(this.totalItems, this.initialPage, this.pageSize);
  }

  private setPage(page: number) {
    // get new pager object for specified page
    this.pager = this.paginate(this.totalItems, page, this.pageSize);

    // get new page of items from items array
    // var pageOfItems = this.items.slice(this.pager.startIndex, this.pager.endIndex + 1);

    // call change page function in parent component
    // this.changePage.emit(pageOfItems);
    this.changePage.emit(page);
    // this.pager = this.paginate(this.totalItems, page, this.pageSize);
  }

  paginate(
    totalItems: number,
    currentPage: number = 1,
    pageSize: number = 10,
    maxPages: number = 10
  ) {
    // calculate total pages
    let totalPages = Math.ceil(totalItems / pageSize);

    // ensure current page isn't out of range
    if (currentPage < 1) {
      currentPage = 1;
    } else if (currentPage > totalPages) {
      currentPage = totalPages;
    }

    let startPage: number, endPage: number;
    if (totalPages <= maxPages) {
      // total pages less than max so show all pages
      startPage = 1;
      endPage = totalPages;
    } else {
      // total pages more than max so calculate start and end pages
      let maxPagesBeforeCurrentPage = Math.floor(maxPages / 2);
      let maxPagesAfterCurrentPage = Math.ceil(maxPages / 2) - 1;
      if (currentPage <= maxPagesBeforeCurrentPage) {
        // current page near the start
        startPage = 1;
        endPage = maxPages;
      } else if (currentPage + maxPagesAfterCurrentPage >= totalPages) {
        // current page near the end
        startPage = totalPages - maxPages + 1;
        endPage = totalPages;
      } else {
        // current page somewhere in the middle
        startPage = currentPage - maxPagesBeforeCurrentPage;
        endPage = currentPage + maxPagesAfterCurrentPage;
      }
    }

    // calculate start and end item indexes
    let startIndex = (currentPage - 1) * pageSize;
    let endIndex = Math.min(startIndex + pageSize - 1, totalItems - 1);

    // create an array of pages to ng-repeat in the pager control
    let pages = Array.from(Array((endPage + 1) - startPage).keys()).map(i => startPage + i);

    // return object with all pager properties required by the view
    return {
      totalItems: totalItems,
      currentPage: currentPage,
      pageSize: pageSize,
      totalPages: totalPages,
      startPage: startPage,
      endPage: endPage,
      startIndex: startIndex,
      endIndex: endIndex,
      pages: pages
    };
  }
}
